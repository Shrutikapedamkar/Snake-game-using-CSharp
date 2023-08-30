using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Snake
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver {  get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        //Stores the grid position occupied by the snake
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        //Random position of the food
        private readonly Random random = new Random();
        private Direction dir;

        public GameState(int rows, int cols) { 
            Rows = rows; 
            Cols = cols;
            Grid = new GridValue[rows, cols];
            //Start the position of the snake from the right
            Dir = Direction.Right;

            AddSnake();
            AddFood();
        }

        //Method to add the snake to the grid
        private void AddSnake()
        {
            //Adding the snake in the middle row and towards the right 
            int r = Rows / 2;
            for(int c = 1; c <=3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }

        //Method to add the food
        //loop throgh the entire grid and ceck if any position(r,c) is empty
        private IEnumerable<Position> EmptyPosition()
        {
            for (int r=0;r< Rows;r++)
            {
                for(int c=0;c< Cols;c++)
                {
                    if (Grid[r,c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPosition());
            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }

        //Snake related helper methods
        //1. Snakes head position
        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }
        //2. Snakes tail position
        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }
        //3. All the snakes position
        public IEnumerable<Position> SnakePosition()
        {
            return snakePositions;
        }
        //4. Adds given position to the head of the snake giving a new head
        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }
        
        private Direction getLastDirection()
        {
            if(dirChanges.Count == 0)
            {
                return Dir;
            }
            return dirChanges.Last.Value;
        }

        private bool canChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }
            Direction lastDir = getLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();

        }
        //Methods to change the game state
        //Change the direction of the snake
        public void ChangeDirection(Direction dir)
        {
            if (canChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }
        
        //Check if the new position is outside the grid
        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        //Takes the position as paramater and checks what it would hit if it moves there
        private GridValue WillHit(Position newHeadPos)
        {
            //if new head position is outside the grid
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }
            //if new head position == current tail position, the game shouls continue
            if(newHeadPos == TailPosition()) {
                return GridValue.Empty;
            }
            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        //Move the snake in current direction
        public void Move()
        {
            if(dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }
            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            
            else if(hit == GridValue.Empty) {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if(hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }

        }

    }
}
