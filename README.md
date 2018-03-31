# Gomoku
AI project done by Claudio Mutti and Hamza Louar

# Objective
Create an AI which can play the Gomoku game, also create a player-vs-player mode where you can ask the help of the AI.

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
Since it's matematically proven that while playong Gomoku, if both players play at their best, that the first player always wins there are some prohibithed moves that can be added to make the game more fair.

## Double Threes
Double threes prevent a player from putting a stone that will create at least two free-threes.

A free three is an align of 3 stones of the same color that are not flanked neither by another stone (even of the same color) nor by the side of the board. It is to note that there can be one empty space (and only one) inside the alignment of the 3 stones and still be called a free-three.

Creating a double three will inevitably lead to a win, so we prevent to create such a strong move to avoid to much lead for the player who starts first.

There is, however, one exception for this prohibition: if a player can do a capture when creating the double-three then he will be able to play in that spot (you can see it as a reward for both creating a double-tree and preparing a capture at the same spot)


## Self captures
Self captures
