using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake
{
    public partial class MainWindow : Window
    {
        const int SnakePartsSize = 20;
        const int SnakeStartingLength = 3;
        const int SnakeStartingSpeed = 200;
        private Random rand = new Random();
        private SolidColorBrush snakeBodyColor = Brushes.Green;
        private SolidColorBrush snakeHeadColor = Brushes.YellowGreen;
        private List<snakePart> snakeBodyParts = new List<snakePart>();
        private UIElement food = null;
        private SolidColorBrush foodBrush = Brushes.Red;
        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength;
        private int score = 0;
        private System.Windows.Threading.DispatcherTimer gameTimer = new System.Windows.Threading.DispatcherTimer();
        
        public MainWindow()
        {
            InitializeComponent();
            gameTimer.Tick += GameTimerTick;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            StartNewGame();
        }
        private void DrawGameArea()
        {
            bool BackgroundDrawn = false;
            int xPosition = 0, yPosition = 0;
            int rowCounter = 0;

            while (BackgroundDrawn == false)
            {
                Rectangle rectangle = new Rectangle
                {
                    Width = SnakePartsSize,
                    Height = SnakePartsSize,
                    Fill = Brushes.Black
                };
                GameArea.Children.Add(rectangle);
                Canvas.SetTop(rectangle, yPosition);
                Canvas.SetLeft(rectangle, xPosition);

                xPosition += SnakePartsSize;
                if (xPosition >= GameArea.ActualWidth)
                {
                    xPosition = 0;
                    yPosition += SnakePartsSize;
                    rowCounter++;
                }

                if (yPosition >= GameArea.ActualHeight)
                    BackgroundDrawn = true;
            }
        }
        private void DrawSnake()
        {
            foreach (snakePart snakeBody in snakeBodyParts)
            {
                if (snakeBody.UiElement == null)
                {
                    snakeBody.UiElement = new Ellipse()
                    {
                        Width = SnakePartsSize,
                        Height = SnakePartsSize,
                        Fill = (snakeBody.IsHead ? snakeHeadColor : snakeBodyColor)
                    };
                    GameArea.Children.Add(snakeBody.UiElement);
                    Canvas.SetTop(snakeBody.UiElement, snakeBody.Position.Y);
                    Canvas.SetLeft(snakeBody.UiElement, snakeBody.Position.X);
                }
            }
        }
        private void MoveSnake()
        {
            while (snakeBodyParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeBodyParts[0].UiElement);
                snakeBodyParts.RemoveAt(0);
            }
            
            foreach (snakePart snakeBody in snakeBodyParts)
            {
                (snakeBody.UiElement as Ellipse).Fill = snakeBodyColor;
                snakeBody.IsHead = false;
            }

            snakePart snakeHead = snakeBodyParts[snakeBodyParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakePartsSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakePartsSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakePartsSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakePartsSize;
                    break;
            }
  
            snakeBodyParts.Add(new snakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            DrawSnake();  
            CollisionCheck();          
        }
        private void GameTimerTick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void StartNewGame()
        {
            foreach (snakePart snakeBodyPart in snakeBodyParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeBodyParts.Clear();
            if (food != null)
                GameArea.Children.Remove(food);

            score = 0;
            snakeLength = SnakeStartingLength;
            snakeDirection = SnakeDirection.Right;
            snakeBodyParts.Add(new snakePart() 
            { 
                Position = new Point(SnakePartsSize * 5, SnakePartsSize * 5) 
            });
            gameTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartingSpeed);

            DrawSnake();
            DrawFood();

            UpdateGameStatus();
      
            gameTimer.IsEnabled = true;
        }

        private Point NextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakePartsSize);
            int maxY = (int)(GameArea.ActualHeight / SnakePartsSize);
            int foodX = rand.Next(0, maxX) * SnakePartsSize;
            int foodY = rand.Next(0, maxY) * SnakePartsSize;

            foreach (snakePart snakePart in snakeBodyParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return NextFoodPosition();
            }

            return new Point(foodX, foodY);
        }
        private void DrawFood()
        {
            Point foodPosition = NextFoodPosition();
            food = new Ellipse()
            {
                Width = SnakePartsSize,
                Height = SnakePartsSize,
                Fill = foodBrush
            };
            GameArea.Children.Add(food);
            Canvas.SetTop(food, foodPosition.Y);
            Canvas.SetLeft(food, foodPosition.X);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }
            if (snakeDirection != originalSnakeDirection)
                MoveSnake();
        }
        private void CollisionCheck()
        {
            snakePart snakeHead = snakeBodyParts[snakeBodyParts.Count - 1];

            if ((snakeHead.Position.X == Canvas.GetLeft(food)) && (snakeHead.Position.Y == Canvas.GetTop(food)))
            {
                SnakeEats();
                return;
            }

            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) ||
            (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }

            foreach (snakePart snakeBodyPart in snakeBodyParts.Take(snakeBodyParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }
        }
        private void SnakeEats()
        {
            snakeLength++;
            score++;
            GameArea.Children.Remove(food);
            DrawFood();
            UpdateGameStatus();
            Image dynamicImage = new Image();
            dynamicImage.Width = 400;
            dynamicImage.Height = 400;
            var path1 = @"https://cdn.pixabay.com/photo/2019/02/06/17/09/snake-3979601_1280.jpg";
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path1);
            bitmap.EndInit();
            dynamicImage.Source = bitmap;
            //GameArea.Children.Add(dynamicImage);
            //DrawGameArea();
            //Stopwatch watch = Stopwatch.StartNew();

            //if (watch.Elapsed.TotalMilliseconds == 15)
            //{
            //    GameArea.Children.Remove(dynamicImage);
            //    DrawGameArea();
            //}
        }
        private void EndGame()
        {
            gameTimer.IsEnabled = false;
            MessageBox.Show("GAME OVER!\n\nPress space button to start new game");
        }
        private void UpdateGameStatus()
        {
            this.Title = "SnakeWPF - Score: " + score;
        }
    }
}
