﻿using SnakeGameML.Interfaces;
using SnakeGameML.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SnakeGameML
{
    public partial class SnakeForm : Form
    {
        private SnakeController _selfPlayer;

        private int _columns = 50, _rows = 25, _score, _dx, _dy, _front, _back;
        private SnakePiece[] _snake = new SnakePiece[1250];
        private List<int> _available = new List<int>();
        private List<FoodPiece> _foodPieces = new List<FoodPiece>();
        private bool[,] _visit;
        private bool _started;

        private const int MAX_FOOD_NUMBER = 5;
        private const int PROBABILITY_OF_GOOD_FOOD = 60;
        private const int TIME_INTERVAL = 100;

        private readonly Random rand = new Random();
        private readonly Timer timer = new Timer();

        public SnakeForm()
        {
            InitializeComponent();
            Initialize();
            LaunchTimer();
        }

        public SnakeForm(SnakeController selfPlayer)
        {
            _selfPlayer = selfPlayer;

            InitializeComponent();
            Initialize();
            LaunchTimer();
        }


        private void LaunchTimer()
        {
            timer.Interval = TIME_INTERVAL;
            timer.Tick += MoveTimer;
            timer.Start();
        }

        private void MoveTimer(object sender, EventArgs e)
        {
            if (_selfPlayer != null)
            {
                if (!_started)
                {
                    SnakeForm_KeyDown(null, new KeyEventArgs(Keys.Right));
                }

                var steering = _selfPlayer.MakeMove();
                if(steering == Steering.right)
                {
                    SnakeForm_KeyDown(null, new KeyEventArgs(Keys.Right));
                }
                else if(steering == Steering.left)
                {
                    SnakeForm_KeyDown(null, new KeyEventArgs(Keys.Left));
                }
            }

            int x = _snake[_front].Location.X, y = _snake[_front].Location.Y;
            
            // If still - NOP
            if (_dx == 0 && _dy == 0)
                return;
            
            // If over board - game over
            if(IsOverBoard(x + _dx, y + _dy))
            {
                timer.Stop();
                MessageBox.Show("Game over");
                return;
            }

            // If Collision
            if(CollisionFood(x + _dx, y + _dy))
            {
                // TODO : Can we collide body on food area ?
                if (HitsBody((y + _dy) / SnakePiece.SidePixelSize, (x + _dx) / SnakePiece.SidePixelSize))
                    return;

                // Body growing
                var head = new SnakePiece(x + _dx, y + _dy);
                _front = (_front - 1 + 1250) % 1250;
                _snake[_front] = head;
                _visit[head.Location.Y / SnakePiece.SidePixelSize, head.Location.X / SnakePiece.SidePixelSize] = true;
                Controls.Add(head);

                RandomFood();

                // Refresh control
                this.Invalidate();
            }
            // No collision
            else
            {
                if (HitsBody((y + _dy) / SnakePiece.SidePixelSize, (x + _dx) / SnakePiece.SidePixelSize))
                    return;

                // Move body
                _visit[_snake[_back].Location.Y / SnakePiece.SidePixelSize, _snake[_back].Location.X / SnakePiece.SidePixelSize] = false;
                _front = (_front - 1 + 1250) % 1250;
                _snake[_front] = _snake[_back];
                _snake[_front].Location = new Point(x + _dx, y + _dy);
                _back = (_back - 1 + 1250) % 1250;
                _visit[(y + _dy) / SnakePiece.SidePixelSize, (x + _dx) / SnakePiece.SidePixelSize] = true;
            }
        }

        //TO USE AFTER ML TRANSITION
        //private void SnakeSelfMovement(MovementPath movementChoice, object sender, KeyEventArgs e)
        //{
        //    _dx = _dy = 0;
        //    switch (movementChoice)
        //    {
        //        case MovementPath.Right:
        //            _dx = SnakePiece.SidePixelSize;
        //            break;
        //        case MovementPath.Left:
        //            _dx = -SnakePiece.SidePixelSize;
        //            break;
        //        case MovementPath.Up:
        //            _dy = -SnakePiece.SidePixelSize;
        //            break;
        //        case MovementPath.Down:
        //            _dy = SnakePiece.SidePixelSize;
        //            break;
        //    }
        //}

        private void SnakeForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Initial
            if (!_started)
            {
                //Start up
                _dy = -SnakePiece.SidePixelSize;
                _dx = 0;
                _started = true;
            }

            switch (e.KeyCode)
            {
                case Keys.Right:
                    // Heading up
                    if (_dy == -SnakePiece.SidePixelSize)
                    {
                        _dx = SnakePiece.SidePixelSize;
                        _dy = 0;
                        break;
                    }
                    // Heading right
                    if (_dx == SnakePiece.SidePixelSize)
                    {
                        _dx = 0;
                        _dy = SnakePiece.SidePixelSize;
                        break;
                    }
                    // Heading down
                    if (_dy == SnakePiece.SidePixelSize)
                    {
                        _dx = -SnakePiece.SidePixelSize;
                        _dy = 0;
                        break;
                    }
                    // Heading left
                    if (_dx == -SnakePiece.SidePixelSize)
                    {
                        _dx = 0;
                        _dy = -SnakePiece.SidePixelSize;
                        break;
                    }
                    //_dx = SnakePiece.SidePixelSize;
                    break;
                case Keys.Left:
                    // Heading up
                    if (_dy == -SnakePiece.SidePixelSize)
                    {
                        _dx = -SnakePiece.SidePixelSize;
                        _dy = 0;
                        break;
                    }
                    // Heading right
                    if (_dx == SnakePiece.SidePixelSize)
                    {
                        _dx = 0;
                        _dy = -SnakePiece.SidePixelSize;
                        break;
                    }
                    // Heading down
                    if (_dy == SnakePiece.SidePixelSize)
                    {
                        _dx = SnakePiece.SidePixelSize;
                        _dy = 0;
                        break;
                    }
                    // Heading left
                    if (_dx == -SnakePiece.SidePixelSize)
                    {
                        _dx = 0;
                        _dy = SnakePiece.SidePixelSize;
                        break;
                    }
                    //_dx = -SnakePiece.SidePixelSize;
                    break;
            }
        }

        private void RandomFood()
        {
            //TODO: avoid situation when all food is red! 

            //do not add if more food than max number
            if (MAX_FOOD_NUMBER < _foodPieces.Count)
                return;

            //how many food to generate?
            int numberOfFood = rand.Next(1, 4);
            
            for (int e = 0; e < numberOfFood; e++)
            {
                CreateFood();
            }
        }

        private void CreateFood()
        {
            //choose coordinates randomly
            var i = rand.Next(_rows);
            var j = rand.Next(_columns);
            var idx = i * _columns + j;

            //TODO: posibility that all generated food will be snake - dead end
            //TODO: posibility that map is filled xD

            // If visted 
            if(_visit[i,j] == true)
            {
                return;
            }

            //choose good or bad food - randomly
            var food = rand.Next(0, 101) < PROBABILITY_OF_GOOD_FOOD ? (FoodPiece)new GoodFood(this.Controls) : new BadFood(this.Controls);

            if (!_visit[i, j] && !_available.Contains(idx))
                _available.Add(idx);
            
            //pixels
            food.foodLabel.Left = (_available.IndexOf(idx) * SnakePiece.SidePixelSize) % this.Width;
            food.foodLabel.Top = (_available.IndexOf(idx) * SnakePiece.SidePixelSize) / this.Width * SnakePiece.SidePixelSize;

            _foodPieces.Add(food);
            return;
        }

        private bool HitsBody(int x, int y)
        {
            if(_visit[x,y])
            {
                timer.Stop();
                MessageBox.Show("Snake hit his body");
                return true;
            }
            return false;
        }

        private bool CollisionFood(int x, int y)
        {
            int scoreValue;

            // If x and y is food
            if (_foodPieces.Any(f => x == f.foodLabel.Location.X && y == f.foodLabel.Location.Y))
            {
                var hitFoodPiece = _foodPieces.Where(f => x == f.foodLabel.Location.X && y == f.foodLabel.Location.Y).Select(p => p).FirstOrDefault();
                scoreValue = hitFoodPiece.scoreValue;

                //Remove food piece as it was hit
                _foodPieces.Remove(hitFoodPiece);
                _available.Remove(hitFoodPiece.foodLabel.Location.Y / SnakePiece.SidePixelSize * _columns + hitFoodPiece.foodLabel.Location.X / SnakePiece.SidePixelSize);
                Controls.Remove(hitFoodPiece.foodLabel);

                UpdateScore(scoreValue);
                return true;
            }
            return false;
        }

        private bool IsOverBoard(int x, int y)
        {
            return x < 0 || y < 0 || x > 980 || y > 480;
        }

        private void Initialize()
        {
            _visit = new bool[_rows, _columns];
            //Start from middle
            var head = new SnakePiece((_columns / 2) * SnakePiece.SidePixelSize, (_rows / 2) * SnakePiece.SidePixelSize);

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _visit[i, j] = false;
                    _available.Add(i * _columns + j);
                }
            }

            RandomFood();
            _visit[head.Location.Y / SnakePiece.SidePixelSize, head.Location.X / SnakePiece.SidePixelSize] = true;
            _available.Remove(head.Location.Y / SnakePiece.SidePixelSize * _columns + head.Location.X / SnakePiece.SidePixelSize);
            Controls.Add(head);
            _snake[_front] = head;
        }

        private void UpdateScore(int value)
        {
            _score += value;
            labelScore.Text = "Score: " + _score.ToString();
        }


    }
}
