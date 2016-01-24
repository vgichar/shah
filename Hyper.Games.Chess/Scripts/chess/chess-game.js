$(function () {
    var moveNumber = 0;

    function writeMoveToPGN(from, to) {
        var activePgn = $("#pgn-container .pgn:last");
        if (moveNumber % 30 == 0) {
            var numPgns = $(".pgn").length;
            var lastClass = "col-xs-" + (12 / numPgns);
            var newClass = "col-xs-" + (12 / (numPgns + 1 ));
            if (numPgns > 4) {
                newClass = "col-xs-3";
            }
            $(".pgn").removeClass(lastClass);
            $(".pgn").addClass(newClass);
            activePgn = $("<div class='pgn " + newClass + "'><table><tr><th>#</th><th colspan='2'>PGN</th></tr></table></div>");
            $("#pgn-container").append(activePgn);
        }
        if (moveNumber % 2 == 0) {
            activePgn.find("table").append("<tr><td>" + parseInt((moveNumber + 2) / 2) + "</td><td>" + from + " - " + to + "</td></tr>");
        } else {
            activePgn.find("table tr:last").append("<td>" + from + " - " + to + "</td></tr>");
        }
        moveNumber++;
    }

    var isMyTurn = false;
    var board,
    game = new Chess(),
    statusEl = $('#status'),
    boardEl = $("#board");

    // do not pick up pieces if the game is over
    // only pick up pieces for the side to move
    var onDragStart = function (source, piece, position, orientation) {
        if (game.game_over() === true ||
            (game.turn() === 'w' && piece.search(/^b/) !== -1) ||
            (game.turn() === 'b' && piece.search(/^w/) !== -1)) {
            return false;
        }
    };

    var onDrop = function (source, target) {
        if (!isMyTurn) return;
        onMoveBroadcast(source, target, undefined);
        myTime.setHours(myTime.getHours() + addTime.getHours());
        myTime.setMinutes(myTime.getMinutes() + addTime.getMinutes());
        myTime.setSeconds(myTime.getSeconds() + addTime.getSeconds());
        updateTimeDate(myTime, "#my-info");
    };

    var onMoveBroadcast = function (source, target, hardMove, promotionPiece) {
        var isPawn = game.get(source).type == 'p';
        var isPromotion = isPawn && (target[1] == '1' || target[1] == '8');

        if (isPromotion && promotionPiece == undefined) {
            isMyTurn = !isMyTurn;
            board.drawPromotionDialog();
            setTimeout(function () {
                $("div[class^='spare-pieces'] img").on("click", function () {
                    isMyTurn = !isMyTurn;
                    var pieceType = $(this).attr("data-piece").toLowerCase()[1];
                    onMoveBroadcast(source, target, true, pieceType);
                    board.destroyPromotionDialog();
                });
            }, 0);
        } else {
            validMove = onMove(source, target, hardMove, promotionPiece);
            if (validMove) {
                if (promotionPiece == undefined) {
                    validMove = gameHubProxy.invoke("Move", source, target);
                } else {
                    validMove = gameHubProxy.invoke("MovePromotion", source, target, promotionPiece);
                }
                if (!validMove) {
                    rollbackMove(target, source);
                }
            }
        }
    };

    var rollbackMove = function (from, to) {
        game.undo();
        board.position(game.fen());
        isMyTurn = true;
    }

    var onMove = function (source, target, hardMove, promotionPiece) {
        // see if the move is legal
        var move = game.move({
            from: source,
            to: target,
            promotion: promotionPiece == undefined ? 'q' : promotionPiece
        });

        // illegal move
        if (move === null) return false;
        startTimer();

        if (hardMove != undefined && hardMove) {
            board.move(source + "-" + target);
        }

        isMyTurn = false;
        writeMoveToPGN(source, target);
        updateStatus();
        if (promotionPiece) {
            board.position(game.fen());
        }
        return true;
    }

    // update the board position after the piece snap
    // for castling, en passant, pawn promotion
    var onSnapEnd = function () {
        board.position(game.fen());
    };

    var updateStatus = function () {
        var status = '';

        var moveColor = 'White';
        if (game.turn() === 'b') {
            moveColor = 'Black';
        }

        // checkmate?
        if (game.in_checkmate() === true) {
            status = 'Game over, ' + moveColor + ' is in checkmate.';
        }

            // draw?
        else if (game.in_draw() === true) {
            status = 'Game over, drawn position';
        }

            // game still on
        else {
            status = moveColor + ' to move';

            // check?
            if (game.in_check() === true) {
                status += ', ' + moveColor + ' is in check';
            }
        }

        statusEl.html(status);
    };

    var cfg = {
        draggable: true,
        position: 'start',
        onDragStart: onDragStart,
        onDrop: onDrop,
        onSnapEnd: onSnapEnd
    };
    board = new ChessBoard(boardEl.attr("id"), cfg);
    updateStatus();

    var startDate = new Date(2000, 1, 1);
    var myTime = new Date(2000, 1, 1, 0, 10, 0, 0);
    var opponentTime = new Date(2000, 1, 1, 0, 10, 0, 0);
    var addTime = new Date(2000, 1, 1, 0, 0, 2, 0);
    var sessionId = null;
    var myColor = "white";
    var timerIntervalId = 0;
    var rating = 0;
    var UIState;
    var connection = $.connection;
    var gameHubProxy = connection.gameHub;

    var UIStates = {
        SearchingForMatch: 0,
        MatchFoundGameNotStarted: 1,
        GameStarted: 2,
        GameEnded: 3, // initial state
        PlayerDisconnected: 4,
        PlayerLeft: 4
    };

    function padNumber(num) {
        num += "";
        while (num.length < 2) {
            num = "0" + num;
        }
        return num;
    }

    function startTimer() {
        if (timerIntervalId == 0) {
            timerIntervalId = setInterval(function () {
                if (isMyTurn) {
                    myTime.setSeconds(myTime.getSeconds() - 1);
                    updateTimeDate(myTime, "#my-info");
                } else {
                    opponentTime.setSeconds(opponentTime.getSeconds() - 1);
                    updateTimeDate(opponentTime, "#other-info");
                }

                if (myTime <= startDate || opponentTime <= startDate) {
                    gameHubProxy.invoke("CheckForTimeout");
                }
            }, 1000);
        }
    }

    function stopTimer() {
        clearInterval(timerIntervalId);
        timerIntervalId = 0;
    }

    function updateTimeDate(timeDate, playerInfoId) {
        var time = padNumber(timeDate.getHours()) + ":" + padNumber(timeDate.getMinutes()) + ":" + padNumber(timeDate.getSeconds());
        if (playerInfoId != undefined) {
            $(playerInfoId).find(".time").text(time);
        }
        return time;
    }


    function updateTime(time, playerInfoId) {
        var timeParts = time.split(":");
        var hours = parseInt(timeParts[0]);
        var minutes = parseInt(timeParts[1]);
        var seconds = parseInt(timeParts[2]);
        if (playerInfoId != undefined) {
            $(playerInfoId).find(".time").text(padNumber(hours) + ":" + padNumber(minutes) + ":" + padNumber(seconds));
        }

        return new Date(2000, 1, 1, hours, minutes, seconds, 0);
    }

    function updateState(newState) {
        if (UIState != newState) {
            UIState = newState;

            switch (UIState) {
                case UIStates.SearchingForMatch: {
                    $("#stop-search").removeClass("hidden");
                    $("#search").addClass("hidden");
                    $("#seek-container").removeClass("hidden");
                    $("#game-status-container").addClass("hidden");
                    break;
                };
                case UIStates.GameEnded: {
                    $("#search").removeClass("hidden");
                    $("#stop-search").addClass("hidden");
                    $("#seek-container").removeClass("hidden");
                    $("#game-status-container").addClass("hidden");
                    stopTimer();
                    break;
                };
                case UIStates.MatchFoundGameNotStarted: {
                    $("#search").addClass("hidden");
                    $("#stop-search").addClass("hidden");
                    $("#seek-container").addClass("hidden");
                    $("#game-status-container").removeClass("hidden");
                    $("#leave-game").removeClass("btn-danger").addClass("btn-info");
                    $("#pgn-container").html("");
                    moveNumber = 0;
                    break;
                };
                case UIStates.GameStarted: {
                    $("#search").addClass("hidden");
                    $("#stop-search").addClass("hidden");
                    $("#seek-container").addClass("hidden");
                    $("#game-status-container").removeClass("hidden");
                    $("#leave-game").addClass("btn-danger").removeClass("btn-info");
                    break;
                };
            };
        }
    }

    function setPlayerInfo(containerId, player, time) {
        var playerInfoConatiner = $(containerId);
        playerInfoConatiner.find(".rating").text(player.Rating);
        playerInfoConatiner.find(".name").text(player.UserName);
        playerInfoConatiner.find(".time").text(time);
    }

    updateState(UIStates.GameEnded);

    gameHubProxy.on("searching", function () {
        toastr.info("searching...");
        updateState(UIStates.SearchingForMatch);
    });

    gameHubProxy.on("gameEnded", function (result) {
        toastr.info(result);
        updateState(UIStates.GameEnded);
    });

    gameHubProxy.on("reportMatch", function (match, moves, whiteTimeLeft, blackTimeLeft) {
        updateState(UIStates.MatchFoundGameNotStarted);
        toastr.success(match.MatchId);
        board.start();
        game.reset();
        isMyTurn = match.White.SessionId == sessionId;
        myColor = isMyTurn ? "white" : "black";

        var self = isMyTurn ? match.White : match.Black;
        var other = !isMyTurn ? match.White : match.Black;

        setPlayerInfo("#my-info", self, match.InitialTime);
        setPlayerInfo("#other-info", other, match.InitialTime);

        myTime = updateTime(match.InitialTime, "#my-info");
        opponentTime = updateTime(match.InitialTime, "#other-info");
        addTime = updateTime(match.AddedTimePerMove);

        if (isMyTurn) {
            board.orientation('white');
        } else {
            board.orientation('black');
        }

        var turn = isMyTurn;
        if (moves != undefined && moves.length > 0) {
            updateState(UIStates.GameStarted);
            for (var m in moves) {
                turn = !turn;
                var move = moves[m];
                if (move.PromotionPiece == undefined) {
                    onMove(move.From, move.To, true);
                } else {
                    onMove(move.From, move.To, true, move.PromotionPiece);
                }
            }

            isMyTurn = turn;
        }

        if (whiteTimeLeft != undefined && blackTimeLeft != undefined) {
            stopTimer();
            if (myColor == "white") {
                myTime = updateTime(whiteTimeLeft, "#my-info");
                opponentTime = updateTime(blackTimeLeft, "#other-info");
            } else {
                myTime = updateTime(blackTimeLeft, "#my-info");
                opponentTime = updateTime(whiteTimeLeft, "#other-info");
            }
            startTimer();
        }
    });

    gameHubProxy.on("moved", function (from, to) {
        updateState(UIStates.GameStarted);
        onMove(from, to, true);
        board.position(game.fen());
        isMyTurn = !isMyTurn;
    });

    gameHubProxy.on("movedPromoted", function (from, to, promotionPiece) {
        updateState(UIStates.GameStarted);
        onMove(from, to, true, promotionPiece);
        board.position(game.fen());
    });

    gameHubProxy.on("playerDisconnected", function (palyerId) {
        updateState(UIStates.PlayerDisconnected);
        toastr.warning(palyerId, "player disconnected");
        $("#claim-win").removeClass("hidden");
    });

    gameHubProxy.on("playerLeft", function (palyerId) {
        updateState(UIStates.PlayerLeft);
        toastr.warning(palyerId, "player left");
        toastr.success("You win!");
    });

    gameHubProxy.on("playerReconnected", function (palyerId) {
        updateState(UIStates.GameStarted);
        toastr.success(palyerId, "player reconnected");
        $("#claim-win").addClass("hidden");
    });

    gameHubProxy.on("initPlayerInfo", function (player) {
        sessionId = player.SessionId;
        rating = Math.max(Math.min(player.Rating, 4000), 0);
        var ratingFrom = Math.max(rating - 100, 0);
        var ratingTo = Math.min(rating + 150, 4000);
        $("#seek-from-rating").val(ratingFrom);
        $("#seek-to-rating").val(ratingTo);

        setPlayerInfo("#my-info", player, "00:10:00");
    });

    gameHubProxy.on("updateTime", function (whiteTimeLeft, blackTimeLeft) {
        stopTimer();
        if (myColor == "white") {
            myTime = updateTime(whiteTimeLeft, "#my-info");
            opponentTime = updateTime(blackTimeLeft, "#other-info");
        } else {
            myTime = updateTime(blackTimeLeft, "#my-info");
            opponentTime = updateTime(whiteTimeLeft, "#other-info");
        }
        if (timerIntervalId > 0) {
            startTimer();
        }
    });

    gameHubProxy.on("drawOffered", function () {
        $("#draw-game").addClass("hidden");
        $("#draw-yes").removeClass("hidden");
        $("#draw-no").removeClass("hidden");
    });

    gameHubProxy.on("drawDenied", function () {
        $("#draw-game").removeClass("hidden");
        $("#draw-yes").addClass("hidden");
        $("#draw-no").addClass("hidden");
    });

    connection.hub.start()
        .done(function () {
            $("#search").on("click", function () {
                var ratingFrom = $("#seek-from-rating").val();
                var ratingTo = $("#seek-to-rating").val();
                var initialTime = $("#seek-initial-time").val();
                var addTime = $("#seek-add-time").val();
                gameHubProxy.invoke("FindMatch", initialTime, addTime, ratingFrom, ratingTo);
            });
            $("#stop-search").on("click", function () {
                updateState(UIStates.GameEnded);
                gameHubProxy.invoke("StopFindMatch");
                toastr.clear();
            });
            $("#leave-game").on("click", function () {
                stopTimer();
                updateState(UIStates.GameEnded);
                gameHubProxy.invoke("LeaveMatch");
            });
            $("#draw-game").click(function () {
                $("#draw-game").addClass("hidden");
                gameHubProxy.invoke("OfferDraw");
            });
            $("#draw-yes").click(function () {
                gameHubProxy.invoke("AcceptDraw");
            });
            $("#draw-no").click(function () {
                gameHubProxy.invoke("DenyDraw");
                $("#draw-game").removeClass("hidden");
                $("#draw-yes").addClass("hidden");
                $("#draw-no").addClass("hidden");
            });
            $("#claim-win").click(function () {
                gameHubProxy.invoke("ClaimWinOnDisconnect");
            });
        })
        .fail(function (e) {
            if (e.source === 'HubException') {
                console.log(e.message + ' : ' + e.data.user);
            } else {
                console.log("Couldn't connect")
            }
        });
});