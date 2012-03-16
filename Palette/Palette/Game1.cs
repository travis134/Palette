using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Xml.Serialization;
using System.IO;

namespace Palette
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Level currentLevel;
        private GameState gameState;
        private Block[][] data;
        private Color[] colorPalette;
        private int backgroundColorIndex;
        private int playerColorIndex;
        private Vector2 playerPosition;
        private Vector2 goalPosition;
        private DrawStyle currentDrawStyle;

        private Texture2D[] colorTextures;
        private Texture2D whiteTexture;

        private Point WINDOW_SIZE = new Point(800, 480);
        private Point BLOCK_SIZE = new Point(10, 10);
        private Vector2 GRAVITY = new Vector2(0, 5);
        private float MAX_RADIUS = 10000f;

        private KeyboardState prevKeyboard, currKeyboard;

        private DateTime levelLastModified;

        private bool reloadLevel;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WINDOW_SIZE.X;
            graphics.PreferredBackBufferHeight = WINDOW_SIZE.Y;
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            base.Initialize();
            currKeyboard = prevKeyboard = Keyboard.GetState();
        }

        private void FillAround(int x, int y)
        {
            if (x >= 0 && x < data.Length && y >= 0 && y < data[0].Length)
            {
                if (data[x][y].ColorIndex == backgroundColorIndex && !data[x][y].IsPersistent)
                {
                    data[x][y].ColorIndex = playerColorIndex;
                    FillAround(x + 1, y);
                    FillAround(x - 1, y);
                    FillAround(x, y + 1);
                    FillAround(x, y - 1);
                }
            }
        }

        /*
         * Creates a new square texture of a given color
         */
        private Texture2D SquareTexture(Color c)
        {
            Texture2D output = new Texture2D(GraphicsDevice, BLOCK_SIZE.X, BLOCK_SIZE.Y);

            Color[] colorData = new Color[(int)(BLOCK_SIZE.X * BLOCK_SIZE.Y)];

            for (int i = 0; i < colorData.Length; i++)
            {
                int x = i % BLOCK_SIZE.X;
                int y = i / BLOCK_SIZE.Y;
 
                switch(currentDrawStyle)
                {

                    case DrawStyle.Solid:
                        colorData[i] = c;
                        break;
                    case DrawStyle.Shaded:
                        if (x == 0 || y == 0)
                        {
                            colorData[i] = new Color((byte)c.R + 55, (byte)c.G + 55, (byte)c.B + 55, (byte)c.A);
                        }
                        else if (x == BLOCK_SIZE.X - 1 || y == BLOCK_SIZE.Y - 1)
                        {
                            colorData[i] = new Color((byte)c.R - 55, (byte)c.G - 55, (byte)c.B - 55, (byte)c.A);
                        }
                        else
                        {
                            colorData[i] = c;
                        }
                        break;
                }
            }

            output.SetData<Color>(colorData);

            return output;
        }

        private Texture2D DecorationTexture(DecorationType decorationType)
        {
            Texture2D output = new Texture2D(GraphicsDevice, BLOCK_SIZE.X, BLOCK_SIZE.Y);

            Color[] colorData = new Color[(int)(BLOCK_SIZE.X * BLOCK_SIZE.Y)];

            for (int i = 0; i < colorData.Length; i++)
            {
                int x = i % BLOCK_SIZE.X;
                int y = i / BLOCK_SIZE.Y;

                switch (decorationType)
                {
                    case DecorationType.None:
                        colorData[i] = Color.Transparent;
                        break;
                    case DecorationType.Player:
                        if (x - 1 >= BLOCK_SIZE.X / 5 && x + 1 < BLOCK_SIZE.X - (BLOCK_SIZE.X / 5) && y - 1 >= BLOCK_SIZE.Y / 5 && y + 1 < BLOCK_SIZE.Y - (BLOCK_SIZE.Y / 5))
                        {
                            colorData[i] = Color.Black;
                        }
                        else
                        {
                            colorData[i] = Color.Transparent;
                        }
                        break;
                }
            }

            output.SetData<Color>(colorData);

            return output;
        }

        private Level LoadLevel(int levelIndex)
        {
            Level result;

            FileStream stream;
            XmlSerializer serializer;

            stream = File.Open("level" + levelIndex.ToString() + ".xml", FileMode.Open, FileAccess.Read);
            serializer = new XmlSerializer(typeof(Level));
            result = (Level)serializer.Deserialize(stream);
            stream.Close();

            return result;
        }

        private Texture2D[] LoadColorTextures(Color[] palette)
        {
            Texture2D[] result = new Texture2D[colorPalette.Length];

            for (int i = 0; i < colorPalette.Length; i++)
            {
                result[i] = SquareTexture(colorPalette[i]);
            }

            return result;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            FileStream stream;
            XmlSerializer serializer;

            if (reloadLevel)
            {
                currentLevel = LoadLevel(gameState.Number);
                currentDrawStyle = currentLevel.DrawStyle;
                colorPalette = currentLevel.ColorPalette;
                whiteTexture = SquareTexture(Color.White);
                colorTextures = LoadColorTextures(colorPalette);
                goalPosition = currentLevel.Goal;
                data = currentLevel.Data;
                reloadLevel = false;
                return;
            }

            int levelIndex = 1;


            /*
            stream = File.Open("level" + levelIndex.ToString() + ".xml", FileMode.Open, FileAccess.Read);
            serializer = new XmlSerializer(typeof(Level));
            currentLevel = (Level)serializer.Deserialize(stream);
            stream.Close();

            for (int i = 0; i < currentLevel.Data.Length; i++)
            {
                for (int j = 0; j < currentLevel.Data[i].Length; j++)
                {
                    if(currentLevel.Data[i][j].ColorIndex == 2)
                    {
                        currentLevel.Data[i][j].IsPersistent = true;
                    }
                }
            }

            stream = File.Open("level1.xml", FileMode.Create);
            serializer = new XmlSerializer(typeof(Level));
            serializer.Serialize(stream, currentLevel);
            stream.Close();
            return;*/
             

            if (File.Exists("gamestate.xml"))
            {
                stream = File.Open("gamestate.xml", FileMode.Open, FileAccess.Read);
                serializer = new XmlSerializer(typeof(GameState));
                gameState = (GameState)serializer.Deserialize(stream);
                stream.Close();

                levelIndex = gameState == null ? 1 : gameState.Number;
            }

            stream = File.Open("level" + levelIndex.ToString() + ".xml", FileMode.Open, FileAccess.Read);
            serializer = new XmlSerializer(typeof(Level));
            currentLevel = (Level)serializer.Deserialize(stream);
            stream.Close();

            levelLastModified = File.GetLastWriteTime("level" + levelIndex.ToString() + ".xml");

            if (gameState != null)
            {
                data = gameState.Data;
                backgroundColorIndex = gameState.BackgroundColorIndex;
                playerColorIndex = gameState.PlayerColorIndex;
                playerPosition = gameState.PlayerPosition;
            }
            else
            {
                data = currentLevel.Data;
                backgroundColorIndex = currentLevel.BackgroundColorIndex;
                playerColorIndex = currentLevel.PlayerColorIndex;
                playerPosition = currentLevel.PlayerStart;

                gameState = new GameState()
                {
                    Number = levelIndex,
                    Data = data,
                    BackgroundColorIndex = backgroundColorIndex,
                    PlayerColorIndex = playerColorIndex,
                    PlayerPosition = playerPosition
                };
            }

            colorPalette = currentLevel.ColorPalette;
            goalPosition = currentLevel.Goal;
            currentDrawStyle = currentLevel.DrawStyle;

            whiteTexture = SquareTexture(Color.White);

            colorTextures = LoadColorTextures(colorPalette);
        }

        protected override void UnloadContent()
        {
            gameState.Number = currentLevel.Number;
            gameState.Data = data;
            gameState.PlayerColorIndex = playerColorIndex;
            gameState.PlayerPosition = playerPosition;
            gameState.BackgroundColorIndex = backgroundColorIndex;

            FileStream stream = File.Open("gamestate.xml", FileMode.Create);
            XmlSerializer serializer = new XmlSerializer(typeof(GameState));
            serializer.Serialize(stream, gameState);
            stream.Close();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            //Reload level when it is modified
            if (File.GetLastWriteTime("level" + gameState.Number.ToString() + ".xml") > levelLastModified)
            {
                levelLastModified = File.GetLastWriteTime("level" + gameState.Number.ToString() + ".xml");
                reloadLevel = true;
                LoadContent();
            }

            currKeyboard = Keyboard.GetState();

            //Gravity
            if ((int)((playerPosition.Y + BLOCK_SIZE.Y) / BLOCK_SIZE.Y) < data[0].Length)
            {
                if (data[(int)(playerPosition.X / BLOCK_SIZE.X)][(int)((playerPosition.Y + BLOCK_SIZE.Y) / BLOCK_SIZE.Y)].ColorIndex == backgroundColorIndex)
                {
                    playerPosition.Y += GRAVITY.Y;
                }
            }


            //Player movement
            if (prevKeyboard.IsKeyDown(Keys.Down))
            {
                if ((int)((playerPosition.Y + BLOCK_SIZE.Y) / BLOCK_SIZE.Y) < data[0].Length)
                {
                    if (data[(int)(playerPosition.X / BLOCK_SIZE.X)][(int)((playerPosition.Y + BLOCK_SIZE.Y) / BLOCK_SIZE.Y)].ColorIndex == backgroundColorIndex)
                    {
                        //Move if player isn't colliding with a block below
                        playerPosition.Y += BLOCK_SIZE.Y;
                    }
                    else
                    {
                        //Acquire color of block that the player collides with
                        playerColorIndex = data[(int)(playerPosition.X / BLOCK_SIZE.X)][(int)((playerPosition.Y + BLOCK_SIZE.Y) / BLOCK_SIZE.Y)].ColorIndex;
                    }
                }
            }
            else if (prevKeyboard.IsKeyDown(Keys.Up))
            {
                if ((int)((playerPosition.Y - BLOCK_SIZE.Y) / BLOCK_SIZE.Y) >= 0)
                {
                    if (data[(int)(playerPosition.X / BLOCK_SIZE.X)][(int)((playerPosition.Y - BLOCK_SIZE.Y) / BLOCK_SIZE.Y)].ColorIndex == backgroundColorIndex)
                    {
                        //Move if player isn't colliding with a block above
                        playerPosition.Y -= BLOCK_SIZE.Y;
                    }
                    else
                    {
                        //Acquire color of block that the player collides with
                        playerColorIndex = data[(int)(playerPosition.X / BLOCK_SIZE.X)][(int)((playerPosition.Y - BLOCK_SIZE.Y) / BLOCK_SIZE.Y)].ColorIndex;
                    }
                }
            }

            if (prevKeyboard.IsKeyDown(Keys.Right))
            {
                if ((int)((playerPosition.X + BLOCK_SIZE.X) / BLOCK_SIZE.X) < data.Length)
                {
                    if (data[(int)((playerPosition.X + BLOCK_SIZE.X) / BLOCK_SIZE.X)][(int)(playerPosition.Y / BLOCK_SIZE.Y)].ColorIndex == backgroundColorIndex)
                    {
                        //Move if player isn't colliding with a block to right
                        playerPosition.X += BLOCK_SIZE.X;
                    }
                    else
                    {
                        //Acquire color of block that the player collides with
                        playerColorIndex = data[(int)((playerPosition.X + BLOCK_SIZE.X) / BLOCK_SIZE.X)][(int)(playerPosition.Y / BLOCK_SIZE.Y)].ColorIndex;
                    }
                }
            }
            else if (prevKeyboard.IsKeyDown(Keys.Left))
            {
                if ((int)((playerPosition.X - BLOCK_SIZE.X) / BLOCK_SIZE.X) >= 0)
                {
                    if (data[(int)((playerPosition.X - BLOCK_SIZE.X) / BLOCK_SIZE.X)][(int)(playerPosition.Y / BLOCK_SIZE.Y)].ColorIndex == backgroundColorIndex)
                    {
                        //Move if player isn't colliding with a block to left
                        playerPosition.X -= BLOCK_SIZE.X;
                    }
                    else
                    {
                        //Acquire color of block that the player collides with
                        playerColorIndex = data[(int)((playerPosition.X - BLOCK_SIZE.X) / BLOCK_SIZE.X)][(int)(playerPosition.Y / BLOCK_SIZE.Y)].ColorIndex;
                    }
                }
            }

            if (currKeyboard.IsKeyUp(Keys.Space) && prevKeyboard.IsKeyDown(Keys.Space))
            {
                FillAround((int)(playerPosition.X / BLOCK_SIZE.X), (int)(playerPosition.Y / BLOCK_SIZE.Y));

                int tempColor = playerColorIndex;

                playerColorIndex = backgroundColorIndex;
                backgroundColorIndex = tempColor;
            }

            //Destroy gamestate
            if (currKeyboard.IsKeyUp(Keys.Escape) && prevKeyboard.IsKeyDown(Keys.Escape))
            {
                File.Delete("gamestate.xml");
                gameState = null;
                LoadContent();
            }

            prevKeyboard = currKeyboard;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < data[x].Length; y++)
                {
                    float distance = (float)(Math.Pow((x * BLOCK_SIZE.X) - playerPosition.X, 2) + Math.Pow((y * BLOCK_SIZE.Y) - playerPosition.Y, 2));
                    if (distance <= MAX_RADIUS && data[x][y].ColorIndex == backgroundColorIndex)
                    {
                        spriteBatch.Draw(whiteTexture, new Rectangle(x * BLOCK_SIZE.X, y * BLOCK_SIZE.Y, BLOCK_SIZE.X, BLOCK_SIZE.Y), null, Color.Lerp(colorPalette[playerColorIndex], colorPalette[backgroundColorIndex], distance / MAX_RADIUS + 0.5f), 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                    }
                    else
                    {
                        spriteBatch.Draw(colorTextures[data[x][y].ColorIndex], new Rectangle(x * BLOCK_SIZE.X, y * BLOCK_SIZE.Y, BLOCK_SIZE.X, BLOCK_SIZE.Y), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                    }
                }
            }

            spriteBatch.Draw(colorTextures[backgroundColorIndex], new Rectangle((int)playerPosition.X, (int)playerPosition.Y, BLOCK_SIZE.X, BLOCK_SIZE.Y), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.2f);
            
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
