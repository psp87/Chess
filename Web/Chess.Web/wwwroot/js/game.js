$(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/hub").build();
    connection.start();

    let playerId;
    let playerName;
    let playerColor;
    let playerOneName;
    let playerTwoName;

    let $playground = document.querySelector('.playground');
    let $statusText = document.querySelector('#status-text');
    let $statusCheck = document.querySelector('#status-check');

    let $whitePointsValue = document.querySelector('#white-points-value');
    let $whiteMoveHistory = document.querySelector('#white-move-history');
    let $blackPawnsTaken = document.querySelector('#black-pawns-taken');
    let $blackKnightsTaken = document.querySelector('#black-knights-taken');
    let $blackBishopsTaken = document.querySelector('#black-bishops-taken');
    let $blackRooksTaken = document.querySelector('#black-rooks-taken');
    let $blackQueensTaken = document.querySelector('#black-queens-taken');

    let $blackPointsValue = document.querySelector('#black-points-value');
    let $blackMoveHistory = document.querySelector('#black-move-history');
    let $whitePawnsTaken = document.querySelector('#white-pawns-taken');
    let $whiteKnightsTaken = document.querySelector('#white-knights-taken');
    let $whiteBishopsTaken = document.querySelector('#white-bishops-taken');
    let $whiteRooksTaken = document.querySelector('#white-rooks-taken');
    let $whiteQueensTaken = document.querySelector('#white-queens-taken');

    $('#find-game').click(function () {
        let name = $('#username').val();
        connection.invoke("FindGame", name);
        document.querySelector('#find-game').disabled = true;
    })

    $('#threefold-draw').click(function () {
        connection.invoke("IsThreefoldDraw");
    })

    $('#resign').click(function () {
        connection.invoke("Resign");
    })

    $('#offer-draw').click(function () {
        let oldText = $statusText.innerText;
        let oldColor = $statusText.style.color;
        $statusText.style.color = "black";
        $statusText.innerText = `Draw request sent!`;
        sleep(1500).then(() => {
            $statusText.style.color = oldColor;
            $statusText.innerText = oldText;
        })
        connection.invoke("OfferDrawRequest");
    })

    connection.on("PlayerJoined", function (player) {
        playerId = player.id;
    })

    connection.on("WaitingList", function () {
        $('#label').html("Waiting for an opponent!");
    })

    connection.on("Start", function (game) {
        if (game.id != "") {
            document.querySelector('.find-box').style.display = "none";
            $playground.style.display = "block";

            playerColor = (playerId == game.player1.id) ? game.player1.color : game.player2.color;
            playerName = (playerId == game.player1.id) ? game.player1.name : game.player2.name;
            playerOneName = game.player1.name;
            playerTwoName = game.player2.name;

            document.querySelector('#white-name').innerText = playerOneName;
            document.querySelector('#black-name').innerText = playerTwoName;
            updateStatus(game.movingPlayer.name);
        }
    })

    connection.on("ChangeOrientation", function () {
        board.orientation('black');
    })

    connection.on("BoardMove", function (source, target) {
        board.move(`${source}-${target}`);
    })

    connection.on("BoardSnapback", function (fen) {
        board.position(fen);
    })

    connection.on("BoardSetPosition", function (fen) {
        board.position(fen);
    })

    connection.on("EnPassantTake", function (pawnPosition, target) {
        board.move(`${target}-${pawnPosition}`, `${pawnPosition}-${target}`);
    })

    connection.on("GameOver", function (player, gameOver) {
        $statusText.style.color = "purple";
        document.querySelector('#board').style.pointerEvents = "none";
        switch (gameOver) {
            case 1: {
                $statusText.innerText = `CHECKMATE! ${player.name.toUpperCase()} WON THE GAME!`;
                $statusCheck.style.display = "none";
            }
                break;
            case 2: $statusText.innerText = `STALEMATE!`;
                break;
            case 3: $statusText.innerText = `DRAW!`;
                break;
            case 4: $statusText.innerText = `DECLARED THREEFOLD REPETITION DRAW BY ${player.name.toUpperCase()}!`;
                break;
            case 5: $statusText.innerText = `FIVEFOLD REPETITION DRAW!`;
                break;
            case 6: $statusText.innerText = `${player.name.toUpperCase()} RESIGNED!`;
                break;
        }
        $('#offer-draw').prop("disabled", true);
        $('#resign').prop("disabled", true);
    })

    connection.on("ThreefoldAvailable", function (player, isAvailable) {
        if (isAvailable) {
            $('#threefold-draw').removeAttr('disabled');
        } else {
            $('#threefold-draw').attr('disabled', 'disabled');
        }
    })

    connection.on("CheckStatus", function (type) {
        if (type == 2) {
            $statusCheck.style.display = "block";
            $statusCheck.innerText = "CHECK!";
        } else {
            $statusCheck.style.display = "none";
        }
    })

    connection.on("InvalidMove", function (type) {
        $statusText.style.color = "red";
        switch (type) {
            case 3: $statusText.innerText = "King is in check!";
                break;
            case 4: $statusText.innerText = "Will open a check!";
                break;
            default: $statusText.innerText = "Invalid Move!";
                break;
        }
        sleep(1200).then(() => {
            $statusText.innerText = "Your Turn!";
            $statusText.style.color = "green";
        })
    })

    connection.on("DrawOffered", function (player) {
        let oldText = $statusText.innerText;
        let oldColor = $statusText.style.color;

        let $yes = document.createElement("button");
        $yes.innerText = "YES";
        $yes.setAttribute("id", "yes-button");
        $yes.classList.add('btn', 'btn-primary');

        let $no = document.createElement("button");
        $no.innerText = "NO";
        $no.setAttribute("id", "no-button");
        $no.classList.add('btn', 'btn-primary');

        $statusText.style.color = "black";
        $statusText.innerText = `Draw offer by ${player.name}! Do you accept?`;

        let $div = document.createElement("div");
        $div.setAttribute("id", "yes-no-buttons")
        $div.appendChild($yes);
        $div.appendChild($no);
        $statusText.appendChild($div);

        $yes.addEventListener("click", function () {
            connection.invoke("OfferDrawAnswer", true);
            $statusText.innerText = oldText;
            $statusText.style.color = oldColor;
        });

        $no.addEventListener("click", function () {
            connection.invoke("OfferDrawAnswer", false);
            $statusText.innerText = oldText;
            $statusText.style.color = oldColor;
        });
    })

    connection.on("DrawOfferRejected", function (player) {
        let oldText = $statusText.innerText;
        let oldColor = $statusText.style.color;
        $statusText.style.color = "black";
        $statusText.innerText = `Rejected by ${player.name}!`;
        sleep(1500).then(() => {
            $statusText.style.color = oldColor;
            $statusText.innerText = oldText;
        })
    })

    connection.on("UpdateTakenFigures", function (movingPlayer, pieceName, points) {
        if (movingPlayer.name == playerOneName) {
            $whitePointsValue.innerText = points;
            switch (pieceName) {
                case "Pawn": $blackPawnsTaken.innerText++;
                    break;
                case "Knight": $blackKnightsTaken.innerText++;
                    break;
                case "Bishop": $blackBishopsTaken.innerText++;
                    break;
                case "Rook": $blackRooksTaken.innerText++;
                    break;
                case "Queen": $blackQueensTaken.innerText++;
                    break;
            }
        } else {
            $blackPointsValue.innerText = points;
            switch (pieceName) {
                case "Pawn": $whitePawnsTaken.innerText++;
                    break;
                case "Knight": $whiteKnightsTaken.innerText++;
                    break;
                case "Bishop": $whiteBishopsTaken.innerText++;
                    break;
                case "Rook": $whiteRooksTaken.innerText++;
                    break;
                case "Queen": $whiteQueensTaken.innerText++;
                    break;
            }
        }
    })

    connection.on("UpdateMoveHistory", function (movingPlayer, moveNotation) {
        let li = document.createElement('li');
        li.classList.add('list-group-item');
        li.innerText = moveNotation;

        if (movingPlayer.name == playerOneName) {
            $whiteMoveHistory.appendChild(li);
            if ($whiteMoveHistory.getElementsByTagName("li").length > 13) {
                $whiteMoveHistory.removeChild($whiteMoveHistory.childNodes[0]);
            }
        } else {
            $blackMoveHistory.appendChild(li);
            if ($blackMoveHistory.getElementsByTagName("li").length > 13) {
                $blackMoveHistory.removeChild($blackMoveHistory.childNodes[0]);
            }
        }
    })

    connection.on("UpdateStatus", function (movingPlayerName) {
        updateStatus(movingPlayerName);
    })

    connection.on("HighlightMove", function (source, target, player) {
        let $source = document.getElementsByClassName("square-" + source);
        let $target = document.getElementsByClassName("square-" + target);
        if (player.name === playerOneName) {
            removeHighlight("black");
            $source[0].className += " highlight-white";
            $target[0].className += " highlight-white";
        } else {
            removeHighlight("white");
            $source[0].className += " highlight-black";
            $target[0].className += " highlight-black";
        }
    })

    function removeHighlight(color) {
        let $squares = document.querySelectorAll(`.highlight-${color}`);
        for (let i = 0; i < $squares.length; i++) {
            $squares[i].className = $squares[i].className.replace(/\bhighlight\b/g, '');
        }
    }

    function updateStatus(name) {
        if (name == playerName) {
            $statusText.innerText = "Your turn!";
            $statusText.style.color = "green";
        } else {
            $statusText.innerText = `${name}'s turn!`;
            $statusText.style.color = "red";
        }
    }

    function sleep(time) {
        return new Promise((resolve) => setTimeout(resolve, time));
    }

    function onDrop(source, target, piece, newPos, oldPos) {
        if (playerColor === 0 && piece.search(/b/) !== -1 ||
            playerColor === 1 && piece.search(/w/) !== -1) {
            return 'snapback';
        }

        if (target.length === 2) {
            let sourceFen = Chessboard.objToFen(oldPos);
            let targetFen = Chessboard.objToFen(newPos);
            connection.invoke("MoveSelected", source, target, sourceFen, targetFen);
        }
    }

    var config = {
        pieceTheme: 'img/chesspieces/wikipedia/{piece}.png',
        draggable: true,
        dropOffBoard: 'snapback',
        showNotation: true,
        onDrop: onDrop,
        moveSpeed: 50,
        position: 'start'
    }

    var board = ChessBoard('board', config);
})
