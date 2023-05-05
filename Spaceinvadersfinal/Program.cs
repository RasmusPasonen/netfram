using System;
using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;


namespace SpaceInvaders
{
    class Program
    {
        // Player class
        public class Player
        {
            public Vector2 position;
            public float speed;
            public int health;
            public int score;
            public List<Bullet> bullets;
            public Texture2D image;

            public Player(Vector2 position, float speed, int health, Texture2D playerimage)
            {

                this.position = position;
                this.speed = speed / 350;
                this.health = health;
                this.score = 0;
                this.bullets = new List<Bullet>();
                this.image = playerimage;

            }

            public void Update(List<Enemy> enemies)
            {

                // Move player left and right
                if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
                    position.X += speed;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
                    position.X -= speed;
                position.X = Math.Clamp(position.X, 20, 1470); //Player cant go outside screen

                // Fire bullets
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                    bullets.Add(new Bullet(new Vector2(position.X, position.Y - 20), new Vector2(0, -10)));

                // Update bullets
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    bullets[i].Update();

                    bool hitEnemy = false;
                    // Check for collisions with enemies
                    for (int j = enemies.Count - 1; j >= 0; j--)
                    {
                        if (Raylib.CheckCollisionCircles(bullets[i].position, 5, enemies[j].position, 20))
                        {
                            // Reduce enemy health
                            enemies[j].health -= 10;
                            score++;
                            bullets.RemoveAt(i);
                            hitEnemy = true;

                            if (enemies[j].health <= 0)
                            {
                                enemies.RemoveAt(j);
                            }

                            break;
                        }
                    }

                    if (!hitEnemy && bullets[i].position.Y < 0)
                        bullets.RemoveAt(i);
                }
            }

            public void Draw()
            {
                float scale = 0.07f; /// image.width;
                Raylib.DrawTextureEx(image, new Vector2(position.X - 20, (int)position.Y - 20), 0f, scale, Color.WHITE);
                Raylib.DrawRectangleLines((int)position.X - 20, (int)position.Y - 20, 40, 40, Color.BLUE);
                foreach (Bullet bullet in bullets)
                    bullet.Draw();

                Raylib.DrawText("Health: " + health, 10, 10, 20, Color.BLACK);
                Raylib.DrawText("Score: " + score, 10, 40, 20, Color.BLACK);
            }
        }

        // Enemy class
        public class Enemy
        {
            public Vector2 position;
            public float speed;
            public int health;
            public float timeSinceLastShot; // new field
            public List<Bullet> bullets; // new field

            public Enemy(Vector2 position, float speed, int health)
            {
                this.position = position;
                this.speed = speed / 50;
                this.health = health;
                this.timeSinceLastShot = 0;
                this.bullets = new List<Bullet>();
            }
            public void turn()
            {
                speed = -speed;
                position.Y += 50; // move down by 20 pixels



            }
            public bool Update(Player player)
            {
                bool musthitwall = false;
                position.X += speed;
                if (position.X > 1500 - 20 || position.X < 20)
                {
                    musthitwall = true;
                }

                timeSinceLastShot += Raylib.GetFrameTime(); // update time elapsed

                if (timeSinceLastShot >= 2 && Raylib.GetRandomValue(0, 100) < 5) // fire bullet randomly
                {
                    Vector2 bulletPosition = new Vector2(position.X, position.Y + 20);
                    Vector2 bulletVelocity = new Vector2(0, 10);
                    bullets.Add(new Bullet(bulletPosition, bulletVelocity));
                    timeSinceLastShot = 0; // reset time elapsed
                }

                // Update bullets
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    bullets[i].Update();

                    if (Raylib.CheckCollisionCircles(bullets[i].position, 5, player.position, 20))
                    {
                        player.health -= 10;
                        bullets.RemoveAt(i);
                    }
                    else if (bullets[i].position.Y > position.Y + 500) // check if bullet is more than 200 pixels away from the enemy
                    {
                        bullets.RemoveAt(i);
                    }
                }
                return musthitwall;
            }

            public void Draw()
            {
                Raylib.DrawRectangle((int)position.X - 20, (int)position.Y - 20, 40, 40, Color.GREEN);
                foreach (Bullet bullet in bullets)
                    bullet.Draw();
            }
        }
        // Bullet class
        public class Bullet
        {
            public Vector2 position;
            public Vector2 velocity;


            public Bullet(Vector2 position, Vector2 velocity)
            {
                this.position = position;
                this.velocity = velocity / 40;

            }

            public void Update()
            {
                position += velocity * 0.3f; //bullet speed halved
            }

            public void Draw()
            {
                Raylib.DrawCircle((int)position.X, (int)position.Y, 5, Color.RED);
            }

        }

        static void Main(string[] args)
        {
            const int screenWidth = 1500;
            const int screenHeight = 850;

            Raylib.InitWindow(screenWidth, screenHeight, "Space Invaders");
            Raylib.SetTargetFPS(2500);

            // Load assets
            Texture2D playerImage = Raylib.LoadTexture("Images/player.png");

            // Create player
            Player player = new Player(new Vector2(screenWidth / 2, screenHeight - 50), 100, 100, playerImage);

            // Create enemies
            const int numRows = 5;
            const int numCols = 10;
            const int enemySpacing = 100;
            const int enemyStartX = 100;
            const int enemyStartY = 50;
            const int enemyHealth = 100;
            const float enemySpeed = 1;

            List<Enemy> enemies = new List<Enemy>();
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    Vector2 enemyPosition = new Vector2(enemyStartX + j * enemySpacing, enemyStartY + i * enemySpacing);
                    enemies.Add(new Enemy(enemyPosition, enemySpeed, enemyHealth));
                }
            }

            // Game loop
            while (!Raylib.WindowShouldClose())
            {
                // Update player
                player.Update(enemies);

                // Update enemies
                bool turnenemies = false;
                foreach (Enemy enemy in enemies)
                {
                    bool turn = enemy.Update(player);
                    if (turn)
                        turnenemies = true;

                }
                if (turnenemies)
                {
                    foreach (Enemy enemy in enemies)
                    {
                        enemy.turn();
                    }

                }

                // Draw
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);

                // Draw player
                player.Draw();

                // Draw enemies
                foreach (Enemy enemy in enemies)
                {
                    enemy.Draw();
                }

                Raylib.EndDrawing();
            }

            // Clean up
            Raylib.UnloadTexture(playerImage);
            Raylib.CloseWindow();
        }
    }
}