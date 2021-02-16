$(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/hub").build();

    let playerId;
    let playerName;
    let playerColor;
    let playerOneName;
    let playerTwoName;
    const elements = {
        playground: document.querySelector('.main-playground'),
        board: document.querySelector('#board'),
        statusText: document.querySelector('.status-bar-text'),
        statusCheck: document.querySelector('.status-bar-check-notification'),
        whiteName: document.querySelector('.main-playground-white-name'),
        whitePointsValue: document.querySelector('.white-points-value'),
        whiteRating: document.querySelector('.main-playground-white-rating'),
        whiteMoveHistory: document.querySelector('.main-playground-white-move-history'),
        blackPawnsTaken: document.querySelector('.taken-pieces-black-pawn-value'),
        blackKnightsTaken: document.querySelector('.taken-pieces-black-knight-value'),
        blackBishopsTaken: document.querySelector('.taken-pieces-black-bishop-value'),
        blackRooksTaken: document.querySelector('.taken-pieces-black-rook-value'),
        blackQueensTaken: document.querySelector('.taken-pieces-black-queen-value'),
        blackName: document.querySelector('.main-playground-black-name'),
        blackRating: document.querySelector('.main-playground-black-rating'),
        blackPointsValue: document.querySelector('.black-points-value'),
        blackMoveHistory: document.querySelector('.main-playground-black-move-history'),
        whitePawnsTaken: document.querySelector('.taken-pieces-white-pawn-value'),
        whiteKnightsTaken: document.querySelector('.taken-pieces-white-knight-value'),
        whiteBishopsTaken: document.querySelector('.taken-pieces-white-bishop-value'),
        whiteRooksTaken: document.querySelector('.taken-pieces-white-rook-value'),
        whiteQueensTaken: document.querySelector('.taken-pieces-white-queen-value'),
        gameChatWindow: document.querySelector('.game-chat-window'),
        lobbyChatWindow: document.querySelector('.game-lobby-chat-window'),
        rooms: document.querySelector('.game-lobby-room-container'),
        lobbyInputName: document.querySelector('.game-lobby-input-name'),
        lobbyInputCreateBtn: document.querySelector('.game-lobby-input-create-btn'),
        lobbyChatInput: document.querySelector('.game-lobby-chat-input'),
        lobbyChatSendBtn: document.querySelector('.game-lobby-chat-send-btn'),
        gameChatInput: document.querySelector('.game-chat-input'),
        gameChatSendBtn: document.querySelector('.game-chat-send-btn'),
        resignBtn: document.querySelector('.resign-btn'),
        offerDrawBtn: document.querySelector('.offer-draw-btn'),
        threefoldDrawBtn: document.querySelector('.threefold-draw-btn'),
    }

    connection.on("AddRoom", function (player) {
        let div = document.createElement('div');
        let span = document.createElement("span");
        let button = document.createElement("button");

        span.innerText = `${player.name.toUpperCase()}'S ROOM`;
        span.classList.add('game-lobby-room-name');
        button.innerText = "JOIN";
        button.classList.add('game-lobby-room-join-btn', 'game-btn', 'btn');

        div.append(span, button);
        div.classList.add(`${player.id}`);
        elements.rooms.appendChild(div);
    })

    connection.on("ListRooms", function (waitingPlayers) {
        $('.game-lobby-room-container').empty();

        waitingPlayers.forEach(player => {
            let div = document.createElement('div');
            let span = document.createElement("span");
            let button = document.createElement("button");

            span.innerText = `${player.name.toUpperCase()}'S ROOM`;
            span.classList.add('game-lobby-room-name');
            button.innerText = "JOIN";
            button.classList.add('game-lobby-room-join-btn', 'game-btn', 'btn');

            div.append(span, button);
            div.classList.add(`${player.id}`);
            elements.rooms.appendChild(div);
        });
    })

    connection.on("Start", function (game) {
        document.querySelector('.game-lobby').style.display = "none";
        elements.playground.style.display = "flex";
        elements.board.style.pointerEvents = "auto";
        $('.game-btn').prop("disabled", false);
        $('.threefold-draw-btn').prop("disabled", true);
        playerColor = (playerId == game.player1.id) ? game.player1.color : game.player2.color;
        playerName = (playerId == game.player1.id) ? game.player1.name : game.player2.name;
        playerOneName = game.player1.name;
        playerTwoName = game.player2.name;
        elements.whiteName.innerHTML = playerOneName;
        elements.blackName.innerHTML = playerTwoName;
        elements.whiteRating.innerHTML = game.player1.rating;
        elements.blackRating.innerHTML = game.player2.rating;
        updateStatus(game.movingPlayer.name);
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
        elements.board.style.pointerEvents = "none";
        switch (gameOver) {
            case 1: {
                elements.statusText.innerText = `CHECKMATE! ${player.name.toUpperCase()} WIN!`;
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
            case 7: elements.statusText.innerText = `${player.name.toUpperCase()} LEFT. YOU WIN!`;
                break;
            case 8: elements.statusText.innerText = `FIFTY-MOVE DRAW!`;
                break;
        }

        $('.option-btn').prop("disabled", true);
    })

    connection.on("ThreefoldAvailable", function (isAvailable) {
        if (isAvailable) {
            $('.threefold-draw-btn').prop('disabled', false);
        } else {
            $('.threefold-draw-btn').prop('disabled', true);
        }
    })

    connection.on("CheckStatus", function (type) {
        if (type == 2) {
            elements.statusCheck.style.display = "inline";
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
        $yes.classList.add('draw-offer-yes-btn', 'draw-offer-button', 'btn', 'btn-primary');

        let $no = document.createElement("button");
        $no.innerText = "NO";
        $no.classList.add('draw-offer-no-btn', 'draw-offer-button', 'btn', 'btn-primary');

        elements.statusText.style.color = "black";
        elements.statusText.innerText = `${player.name} requested a draw! Do you accept?`;

        let $div = document.createElement("div");
        $div.classList.add('draw-offer-container');
        $div.append($yes, $no);
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
        elements.statusText.innerText = `${player.name} rejected the offer!`;
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
            if (elements.whiteMoveHistory.getElementsByTagName("li").length > 40) {
                elements.whiteMoveHistory.removeChild(elements.whiteMoveHistory.childNodes[0]);
            }
        } else {
            elements.blackMoveHistory.appendChild(li);
            if (elements.blackMoveHistory.getElementsByTagName("li").length > 40) {
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

    connection.on("UpdateGameChat", function (message, player) {
        let isBlack = player.name === playerOneName ? false : true;
        updateChat(message, elements.gameChatWindow, false, isBlack);
    })

    connection.on("UpdateGameChatInternalMessage", function (message) {
        updateChat(message, elements.gameChatWindow, true, false);
    })

    connection.on("UpdateLobbyChat", function (message) {
        updateChat(message, elements.lobbyChatWindow, false, false);
    })

    connection.on("UpdateLobbyChatInternalMessage", function (message) {
        updateChat(message, elements.lobbyChatWindow, true, false);
    })

    function updateChat(message, chat, isInternalMessage, isBlack) {
        let li = document.createElement('li');
        li.innerText = `${message}`;
        if (isInternalMessage) {
            li.classList.add('chat-internal-msg', 'chat-msg', 'flex-start');
        } else {
            if (isBlack) {
                li.classList.add('black-chat-msg', 'chat-user-msg', 'chat-msg', 'flex-end');
            } else {
                li.classList.add('white-chat-msg', 'chat-user-msg', 'chat-msg', 'flex-start');
            }
        }

        chat.appendChild(li);
        chat.scrollTop = elements.gameChatWindow.scrollHeight;
    }

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

    connection.start();

    window.addEventListener("beforeunload", function (e) {
        if (playerTwoName !== undefined) {
            e.preventDefault(); // If you prevent default behavior in Mozilla Firefox prompt will always be shown
            e.returnValue = '';
        }
    });

    $(document).on('click', '.game-lobby-room-join-btn', function () {
        let id = $(this).parent().attr('class');
        let name = elements.lobbyInputName.value;
        if (name !== "") {
            connection.invoke("JoinRoom", name, id)
                .then((player) => {
                    playerId = player.id;
                    board.orientation('black');
                })
                .catch((err) => alert(err));
        }
        else {
            elements.lobbyInputName.focus();
        }
    })

    elements.lobbyInputCreateBtn.addEventListener("click", function () {
        let name = elements.lobbyInputName.value;
        if (name !== "") {
            connection.invoke("CreateRoom", name)
                .then((player) => {
                    playerId = player.id;
                    document.querySelector('.game-lobby').style.display = "none";
                    elements.playground.style.display = "flex";
                    elements.board.style.pointerEvents = "none";
                    $('.game-btn').prop("disabled", true);
                    elements.whiteName.innerHTML = player.name;
                    elements.whiteRating.innerHTML = player.rating;
                    elements.blackName.innerHTML = "?";
                    elements.blackRating.innerHTML = "N/A";
                    elements.statusText.style.color = "red";
                    elements.statusText.innerText = "WAITING FOR OPPONENT...";
                })
                .catch((err) => alert(err));
        }
        else {
            elements.lobbyInputName.focus();
        }
    })

    elements.threefoldDrawBtn.addEventListener("click", function () {
        connection.invoke("ThreefoldDraw")
            .catch((err) => alert(err));
    })

    elements.offerDrawBtn.addEventListener("click", function () {
        let oldText = elements.statusText.innerText;
        let oldColor = elements.statusText.style.color;
        elements.statusText.style.color = "black";
        elements.statusText.innerText = `Draw request sent!`;
        sleep(1500).then(() => {
            elements.statusText.style.color = oldColor;
            elements.statusText.innerText = oldText;
        })
        connection.invoke("OfferDrawRequest")
            .catch((err) => alert(err));
    })

    elements.resignBtn.addEventListener("click", function () {
        connection.invoke("Resign")
            .catch((err) => alert(err));
    })

    elements.lobbyChatSendBtn.addEventListener("click", function () {
        let message = elements.lobbyChatInput.value;
        if (message !== "") {
            connection.invoke('LobbySendMessage', message)
                .then(elements.lobbyChatInput.value = "")
                .catch((err) => alert(err));
        }
        else {
            elements.lobbyChatInput.focus();
        }
    })

    elements.gameChatSendBtn.addEventListener("click", function () {
        let message = elements.gameChatInput.value;
        if (message !== "") {
            connection.invoke('GameSendMessage', message)
                .then(elements.gameChatInput.value = "")
                .catch((err) => alert(err));
        }
        else {
            elements.gameChatInput.focus();
        }
    })

    elements.lobbyChatInput.addEventListener("keyup", function (e) {
        if (e.keyCode === 13) {
            elements.lobbyChatSendBtn.click();
        }
    });

    elements.gameChatInput.addEventListener("keyup", function (e) {
        if (e.keyCode === 13) {
            elements.gameChatSendBtn.click();
        }
    });

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
