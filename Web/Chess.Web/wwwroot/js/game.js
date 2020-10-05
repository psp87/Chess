$(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/hub").build();
    connection.start();

    const playerId;
    const playerName;
    const playerColor;
    const playerOneName;
    const playerTwoName;
    const elements = {
        playground: document.querySelector('.playground'),
        statusText = document.querySelector('#status-text'),
        statusCheck = document.querySelector('#status-check'),
        whitePointsValue = document.querySelector('#white-points-value'),
        whiteMoveHistory = document.querySelector('#white-move-history'),
        blackPawnsTaken = document.querySelector('#black-pawns-taken'),
        blackKnightsTaken = document.querySelector('#black-knights-taken'),
        blackBishopsTaken = document.querySelector('#black-bishops-taken'),
        blackRooksTaken = document.querySelector('#black-rooks-taken'),
        blackQueensTaken = document.querySelector('#black-queens-taken'),
        blackPointsValue = document.querySelector('#black-points-value'),
        blackMoveHistory = document.querySelector('#black-move-history'),
        whitePawnsTaken = document.querySelector('#white-pawns-taken'),
        whiteKnightsTaken = document.querySelector('#white-knights-taken'),
        whiteBishopsTaken = document.querySelector('#white-bishops-taken'),
        whiteRooksTaken = document.querySelector('#white-rooks-taken'),
        whiteQueensTaken = document.querySelector('#white-queens-taken'),
    }

    $('#find-game').addEventListener('click', function () {
        let name = $('#username').val();
        connection.invoke("FindGame", name);
        document.querySelector('#find-game').disabled = true;
    })

    $('#threefold-draw').addEventListener('click', function () {
        connection.invoke("IsThreefoldDraw");
    })

    $('#resign').addEventListener('click', function () {
        connection.invoke("Resign");
    })

    $('#offer-draw').addEventListener('click', function () {
        let oldText = elements.statusText.innerText;
        let oldColor = elements.statusText.style.color;
        elements.statusText.style.color = "black";
        elements.statusText.innerText = `Draw request sent!`;
        sleep(1500).then(() => {
            elements.statusText.style.color = oldColor;
            elements.statusText.innerText = oldText;
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
            elements.playground.style.display = "block";
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
        elements.statusText.style.color = "purple";
        document.querySelector('#board').style.pointerEvents = "none";
        switch (gameOver) {
            case 1: {
                elements.statusText.innerText = `CHECKMATE! ${player.name.toUpperCase()} WON THE GAME!`;
                elements.statusCheck.style.display = "none";
            }
                break;
            case 2: elements.statusText.innerText = `STALEMATE!`;
                break;
            case 3: elements.statusText.innerText = `DRAW!`;
                break;
            case 4: elements.statusText.innerText = `DECLARED THREEFOLD REPETITION DRAW BY ${player.name.toUpperCase()}!`;
                break;
            case 5: elements.statusText.innerText = `FIVEFOLD REPETITION DRAW!`;
                break;
            case 6: elements.statusText.innerText = `${player.name.toUpperCase()} RESIGNED!`;
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
            elements.statusCheck.style.display = "block";
            elements.statusCheck.innerText = "CHECK!";
        } else {
            elements.statusCheck.style.display = "none";
        }
    })

    connection.on("InvalidMove", function (type) {
        elements.statusText.style.color = "red";
        switch (type) {
            case 3: elements.statusText.innerText = "King is in check!";
                break;
            case 4: elements.statusText.innerText = "Will open a check!";
                break;
            default: elements.statusText.innerText = "Invalid Move!";
                break;
        }
        sleep(1200).then(() => {
            elements.statusText.innerText = "Your Turn!";
            elements.statusText.style.color = "green";
        })
    })

    connection.on("DrawOffered", function (player) {
        let oldText = elements.statusText.innerText;
        let oldColor = elements.statusText.style.color;

        let $yes = document.createElement("button");
        $yes.innerText = "YES";
        $yes.setAttribute("id", "yes-button");
        $yes.classList.add('btn', 'btn-primary');

        let $no = document.createElement("button");
        $no.innerText = "NO";
        $no.setAttribute("id", "no-button");
        $no.classList.add('btn', 'btn-primary');

        elements.statusText.style.color = "black";
        elements.statusText.innerText = `Draw offer by ${player.name}! Do you accept?`;

        let $div = document.createElement("div");
        $div.setAttribute("id", "yes-no-buttons")
        $div.appendChild($yes);
        $div.appendChild($no);
        elements.statusText.appendChild($div);

        $yes.addEventListener("click", function () {
            connection.invoke("OfferDrawAnswer", true);
            elements.statusText.innerText = oldText;
            elements.statusText.style.color = oldColor;
        });

        $no.addEventListener("click", function () {
            connection.invoke("OfferDrawAnswer", false);
            elements.statusText.innerText = oldText;
            elements.statusText.style.color = oldColor;
        });
    })

    connection.on("DrawOfferRejected", function (player) {
        let oldText = elements.statusText.innerText;
        let oldColor = elements.statusText.style.color;
        elements.statusText.style.color = "black";
        elements.statusText.innerText = `Rejected by ${player.name}!`;
        sleep(1500).then(() => {
            elements.statusText.style.color = oldColor;
            elements.statusText.innerText = oldText;
        })
    })

    connection.on("UpdateTakenFigures", function (movingPlayer, pieceName, points) {
        if (movingPlayer.name == playerOneName) {
            elements.whitePointsValue.innerText = points;
            switch (pieceName) {
                case "Pawn": elements.blackPawnsTaken.innerText++;
                    break;
                case "Knight": elements.blackKnightsTaken.innerText++;
                    break;
                case "Bishop": elements.blackBishopsTaken.innerText++;
                    break;
                case "Rook": elements.blackRooksTaken.innerText++;
                    break;
                case "Queen": elements.blackQueensTaken.innerText++;
                    break;
            }
        } else {
            elements.blackPointsValue.innerText = points;
            switch (pieceName) {
                case "Pawn": elements.whitePawnsTaken.innerText++;
                    break;
                case "Knight": elements.whiteKnightsTaken.innerText++;
                    break;
                case "Bishop": elements.whiteBishopsTaken.innerText++;
                    break;
                case "Rook": elements.whiteRooksTaken.innerText++;
                    break;
                case "Queen": elements.whiteQueensTaken.innerText++;
                    break;
            }
        }
    })

    connection.on("UpdateMoveHistory", function (movingPlayer, moveNotation) {
        let li = document.createElement('li');
        li.classList.add('list-group-item');
        li.innerText = moveNotation;

        if (movingPlayer.name == playerOneName) {
            elements.whiteMoveHistory.appendChild(li);
            if (elements.whiteMoveHistory.getElementsByTagName("li").length > 13) {
                elements.whiteMoveHistory.removeChild(elements.whiteMoveHistory.childNodes[0]);
            }
        } else {
            elements.blackMoveHistory.appendChild(li);
            if (elements.blackMoveHistory.getElementsByTagName("li").length > 13) {
                elements.blackMoveHistory.removeChild(elements.blackMoveHistory.childNodes[0]);
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
            elements.statusText.innerText = "Your turn!";
            elements.statusText.style.color = "green";
        } else {
            elements.statusText.innerText = `${name}'s turn!`;
            elements.statusText.style.color = "red";
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
