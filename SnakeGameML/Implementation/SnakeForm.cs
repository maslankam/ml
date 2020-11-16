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
        private int _columns = 50, _rows = 25, _score, _dx, _dy, _front, _back;
        private SnakePiece[] _snake = new SnakePiece[1250];
        private List<int> _available = new List<int>();
        private List<FoodPiece> _foodPieces = new List<FoodPiece>();
        private bool[,] _visit;
        
        private const int MAX_FOOD_NUMBER = 5;
        private const int PROBABILITY_OF_GOOD_FOOD = 60;
        private const int TIME_INTERVAL = 100;

        private readonly Random rand = new Random();
        private readonly Timer timer = new Timer();

        public  SnakeForm()
        {
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
            int x = _snake[_front].Location.X, y = _snake[_front].Location.Y;
            if (_dx == 0 && _dy == 0)
                return;
            if(IsOverBoard(x + _dx, y + _dy))
            {
                timer.Stop();
                MessageBox.Show("Game over");
                return;
            }

            if(CollisionFood(x + _dx, y + _dy, out int scoreValue))
            {
                _score += scoreValue;
                labelScore.Text = "Score: " + _score.ToString();
                if (Hits((y + _dy) / SnakePiece.SideSize, (x + _dx) / SnakePiece.SideSize))
                    return;
                SnakePiece head = new SnakePiece(x + _dx, y + _dy);
                _front = (_front - 1 + 1250) % 1250;
                _snake[_front] = head;
                _visit[head.Location.Y / SnakePiece.SideSize, head.Location.X / SnakePiece.SideSize] = true;
                Controls.Add(head);
                RandomFood();
                this.Invalidate();
            }
            else
            {
                if (Hits((y + _dy) / SnakePiece.SideSize, (x + _dx) / SnakePiece.SideSize))
                    return;
                _visit[_snake[_back].Location.Y / SnakePiece.SideSize, _snake[_back].Location.X / SnakePiece.SideSize] = false;
                _front = (_front - 1 + 1250) % 1250;
                _snake[_front] = _snake[_back];
                _snake[_front].Location = new Point(x + _dx, y + _dy);
                _back = (_back - 1 + 1250) % 1250;
                _visit[(y + _dy) / SnakePiece.SideSize, (x + _dx) / SnakePiece.SideSize] = true;
            }
        }

        //TO USE AFTER ML TRANSITION
        private void SnakeSelfMovement(MovementPath movementChoice, object sender, KeyEventArgs e)
        {
            _dx = _dy = 0;
            switch (movementChoice)
            {
                case MovementPath.Right:
                    _dx = SnakePiece.SideSize;
                    break;
                case MovementPath.Left:
                    _dx = -SnakePiece.SideSize;
                    break;
                case MovementPath.Up:
                    _dy = -SnakePiece.SideSize;
                    break;
                case MovementPath.Down:
                    _dy = SnakePiece.SideSize;
                    break;
            }
        }

        private void SnakeForm_KeyDown(object sender, KeyEventArgs e)
        {
            _dx = _dy = 0;
            switch (e.KeyCode)
            {
                case Keys.Right:
                    _dx = SnakePiece.SideSize;
                    break;
                case Keys.Left:
                    _dx = -SnakePiece.SideSize;
                    break;
                case Keys.Up:
                    _dy = -SnakePiece.SideSize;
                    break;
                case Keys.Down:
                    _dy = SnakePiece.SideSize;
                    break;
            }
        }

        private void RandomFood()
        {
            //do not add if more food than max number
            if (MAX_FOOD_NUMBER < _foodPieces.Count)
                return;

            int numberOfFood = rand.Next(1, 4);
            
            for (int e = 0; e < numberOfFood; e++)
            {
                CreateFood();
            }
        }

        private void CreateFood()
        {
            var food = rand.Next(0, 101) < PROBABILITY_OF_GOOD_FOOD ? (FoodPiece)new GoodFood(this.Controls) : new BadFood(this.Controls);
            var i = rand.Next(_rows);
            var j = rand.Next(_columns);
            var idx = i * _columns + j;
            if (!_visit[i, j] && !_available.Contains(idx))
                _available.Add(idx);
            
            food.foodLabel.Left = (_available.IndexOf(idx) * SnakePiece.SideSize) % Width;
            food.foodLabel.Top = (_available.IndexOf(idx) * SnakePiece.SideSize) / Width * SnakePiece.SideSize;

            _foodPieces.Add(food);

            return;
        }

        private bool Hits(int x, int y)
        {
            if(_visit[x,y])
            {
                timer.Stop();
                MessageBox.Show("Snake hit his body");
                return true;
            }
            return false;
        }

        private bool CollisionFood(int x, int y, out int scoreValue)
        {
            scoreValue = default;
            if (_foodPieces.Any(f => x == f.foodLabel.Location.X && y == f.foodLabel.Location.Y))
            {
                var hitFoodPiece = _foodPieces.Where(f => x == f.foodLabel.Location.X && y == f.foodLabel.Location.Y).Select(p => p).FirstOrDefault();
                scoreValue = hitFoodPiece.scoreValue;

                //Remove food piece as it was hit
                _foodPieces.Remove(hitFoodPiece);
                _available.Remove(hitFoodPiece.foodLabel.Location.Y / SnakePiece.SideSize * _columns + hitFoodPiece.foodLabel.Location.X / SnakePiece.SideSize);
                Controls.Remove(hitFoodPiece.foodLabel);
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
            SnakePiece head = new SnakePiece((rand.Next() % _columns) * SnakePiece.SideSize, (rand.Next() % _rows) * SnakePiece.SideSize);

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _visit[i, j] = false;
                    _available.Add(i * _columns + j);
                }
            }

            RandomFood();
            _visit[head.Location.Y / SnakePiece.SideSize, head.Location.X / SnakePiece.SideSize] = true;
            _available.Remove(head.Location.Y / SnakePiece.SideSize * _columns + head.Location.X / SnakePiece.SideSize);
            Controls.Add(head);
            _snake[_front] = head;
        }
    }
}
