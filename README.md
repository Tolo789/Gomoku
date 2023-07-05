# Gomoku
AI and online project done by Claudio Mutti and Hamza

# Objective
Create an AI which can play the Gomoku game (a 2 players turn-based board game), also create a player-vs-player mode where you can ask the help of the AI.

#### Gomoku game
![Alt text](/screenshots/gomoku.png?raw=true "Gomoku")

# Project's Constraints
The AI must have the following constraints:
- It must use an algorithm from the MinMax family (we used an Alpha-beta pruning)
- Each of its moves must be played under 0.5 seconds

# Basic rules
- You can capture pairs (and only pairs) of enemy stones by blocking both side of an alignment of 2 enemy stones
- If you manage to do 5 captures (thus capturing 10 stones) you instantly win
- If you align 5 or more stones of the same color (in any direction) you instantly win only if the other player cannot break the align with a capture and cannot reach 5 captures with a capture anywhere in the board. If there is the possibility of doing at least one of the mentioned captures then a supplementary turn is given to the other player, if the player actually do the capture then the game will go on (otherwise it will be a win by align for the player who did the align)

# Offline game
If you want to play against the AI or another local player, then follow this steps:
- First of all click on "settings", here you can configure the AI strength and toggle the use of some prohibited move (see "Prohibited moves" below).
- Then chose "play offline" from the main menu, you will be able to choose if it's a game against the AI or another human as well as if there are some opening handicaps (see "Opening handicaps" below)
- Finally click the "go" button and the game will start, you will be able to ask for the AI help and to undo the last move at any moment of the game

# Online game
To play online you must be connected to the internet and both players must have a version of the game on their computers.
Here is how to launch an online game:
- Both will have to click on "play online" from the main menu
- One will need to create a room (all game configurations can be setted when choosing the name of the room) while the other will have to click on "list rooms" and find the name of the room created by the first player
- After entering a room you can modify both your name and your stone color (click multiple times on the color the cycle between all choices)
- If you are the creator of the room you can kick the other player at any moment
- When your choice for name and color is set click on the "ready button", when both players are ready a countdown will appear before starting the game
- Once the game started only the player who has to play can actually put a stone
- You can ask the AI help only during your turn, only the player who just put his stone can ask for an undo
- For both AI help and undo move (as well as for the restart options) both players have to agree in order to validate it, a special panel will be shown with the "accept" or "deny" choices

# Prohibited moves
When playing Gomoku with the standard rules, the first player is assured to win (as long as both players play at their best).
In order to make the game more fair the following special moves can be set as prohibited.

## Double Threes
Double threes prevent a player from putting a stone that will create at least two free-threes.

#### Exemple of Double-three (black player)
![Alt text](/screenshots/Double-three.png?raw=true "Double-three")

A free-three is an align of 3 stones of the same color that are not flanked neither by another stone (even of the same color) nor by the side of the board. It is to note that there can be one empty space (and only one) inside the alignment of the 3 stones and still be called a free-three.

#### Exemple of free-three (black player)
![Alt text](/screenshots/free-three.png?raw=true "free-three")

#### Exemple of free-three with a space inside (black player)
![Alt text](/screenshots/free-three_2.png?raw=true "another free-three")

There is, however, one exception to this prohibition: if the same move that will create a double three is also a capture then the move is permitted.

#### Exemple of double-three exception (black player)
![Alt text](/screenshots/Double-three_exception.png?raw=true "Double-three exception")

Notes: In the Renju variant of Gomoku the double threes moves are prohibited only for the first player, but for this project both players must not be able to do such move.

## Self captures
"Self capture" is an handicap imagined by ourselves that can actually bring some interesting twists to the game: if there are two stones of the same color with strictly two spaces between them, the other player will only be able to put one of his stones in-between the two enemy stones but not two of them.

#### Exemple of self-capture (white player)
![Alt text](/screenshots/self-capture.png?raw=true "self-capture")

# Opening handicaps
When playing Gomoku with the standard rules, the first player is assured to win (as long as both players play at their best).
In order to make the game more fair, the following rules will reduce the capability of the first player to take control over the board within the first moves.

## Pro and LongPro
Both Pro and LongPro opening handicaps try to reduce the first player influence by forcing to play his second stone (the third stone overall) far from the middle of the board.

With the Pro opening the player must put his stones at least two row and columns away from the center, while with the LongPro variant the move must be at least four rows and columns away.

#### Pro restriction
![Alt text](/screenshots/Pro.png?raw=true "Pro opening")

#### LongPro restriction
![Alt text](/screenshots/LongPro.png?raw=true "LongPro opening")

## Swap and Swap2
Swap and Swap2 openings, instead of restraining the first player moves, take a completely different approach:
- The first player put the first three stones on the board (2 black and 1 white)
- The second player is then asked to choose if he wants to keep playing as a white or to swap his color with the other player, if the second option is chosen then it's the first player who will have to play as white until the end of the game
- Swap2 only: the player will also be able to choose if he prefers to put two more stones (1 white and 1 black) and then let the first player decide if swap colors or not
