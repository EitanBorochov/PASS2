// Author: Eitan Borochov
// File name: Game1.cs
// Project name: PASS2
// Creation Date: Apr. 25th 2025
// Modification Date: May. 7th 2025
// Description: This two player game merges Pong and Brick Breaker.
// Players control vertical Pipes to deflect a bouncing Shell from their edge, which are guarded by destroyable blocks.
// To win you must either have 15 points or the one with the highest score after 60 seconds.
// ALL LEVEL 4 ITEMS ARE COMPLETE

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using GameUtility;

namespace PASS2;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Storing screen dimensions
    int screenWidth;
    int screenHeight;
    
    // Storing game state constants
    const byte MENU = 0;
    const byte PRE_LAUNCH = 1;
    const byte PLAY = 2;
    const byte PAUSE = 3;
    const byte ENDGAME = 4;
    
    // Storing the current game state
    byte gameState = MENU;
    
    // Initializing new instance of random number generator
    Random rng = new Random();
    
    // Storing keyboard input
    KeyboardState kb;
    KeyboardState prevKb;
    
    // Storing spritefonts for UI
    SpriteFont smallFont;
    SpriteFont mediumFont;
    SpriteFont largeFont;
    
    // storing title image and rectangle
    Texture2D titleImg;
    Rectangle titleRec;
    
    // Storing drop shadow constant
    readonly Vector2 SHADOW_OFFSET = new Vector2(5, 5);
    
    // storing PRESS ENTER Text which will be the same for all game states except menu
    const string ENTER_TEXT = "PRESS ENTER";
    Vector2 enterPos;
    
    // Storing press enter and press space text and position for menu
    const string ENTER_TEXT_MENU = "PRESS ENTER - 1v1";
    const string SPACE_TEXT_MENU = "PRESS SPACE - CPU";
    Vector2 enterMenuPos;
    Vector2 spaceMenuPos;
    
    // Storing paused title text and location
    const string PAUSE_TITLE = "PAUSED";
    Vector2 pausePos;
    
    // Storing end game title text and location
    const string ENDGAME_TITLE = "GAME OVER";
    Vector2 endgamePos;
    
    // Storing HUD brick image and array of rectangles
    Texture2D brickImg;
    Rectangle[] topBrickRecs = new Rectangle[16];
    Rectangle[] bottomBrickRecs = new Rectangle[16];
    
    // Storing question block image, array of rectangles, and arrays of colors
    Texture2D questionImg;
    Rectangle[] leftQuestRecs = new Rectangle[7];
    Rectangle[] rightQuestRecs = new Rectangle[7];
    Color[] leftQuestColor = new Color[7];
    Color[] rightQuestColor = new Color[7];
    
    // Storing textures and rectangle for pipes (left is 0, right is 1)
    Texture2D leftPipeImg;
    Texture2D rightPipeImg;
    Rectangle[] pipeRecs = new Rectangle[2];
    
    // Storing player pipe speed
    const int PIPE_SPEED = 6;
    
    // Storing shell texture, position, rectangle, and velocity
    Texture2D shellImg;
    Rectangle shellRec;
    Vector2 shellPos;
    Vector2 shellVel;
    
    // Storing shell midpoints for collisions
    Vector2 shellMidTop;
    Vector2 shellLeftMid;
    Vector2 shellRightMid;
    Vector2 shellMidBottom;
    
    // Storing scores for both players as well as positions for display
    int scoreLeft;
    int scoreRight;
    Vector2 scoreLeftPos;
    Vector2 scoreRightPos;
    
    // storing winning player and its position
    string winnerDisp = "0";
    Vector2 winnerPos;
    
    // Adding game timer and game timer display position
    Timer gameTimer = new Timer(60000, false);
    string timeLeftDisp = "60";
    Vector2 gameTimerPos;
    
    // Storing timer for star buff
    Timer starTimer = new Timer(10000, false);
    
    // Storing coin image, animation, and position
    Texture2D coinImg;
    Animation coinLeftAnim;
    Animation coinRightAnim;
    Vector2 coinLeftPos;
    Vector2 coinRightPos;
    
    // Storing image, animation, and position (same as initial pos of shell) for rotating star
    Texture2D starImg;
    Animation starAnim;
    Vector2 starPos;
    
    // Tracking which player was last to hit the shell (o - left, 1 - right, 2 - no one)
    byte lastShellHit = 2;
    
    // Storing number of players playing (1 or 2)
    byte numPlayers;
    
    // Storing future shell position for AI player
    float futureYShellPos;
    
    // Storing player statistics
    int gamesPlayed = 0;
    int winsLeft = 0;
    int winsRight = 0;
    float winPercentLeft = 0;
    float winPercentRight = 0;
    
    // Storing strings and positions for statistics displays
    string leftStatsDisp;
    string rightStatsDisp;
    string gamesPlayedDisp;
    
    Vector2 leftStatsPos;
    Vector2 rightStatsPos;
    Vector2 gamesPlayedPos;
    
    // Storing music for each game state
    Song menuMusic;
    Song playMusic;
    Song pauseMusic;
    Song endgameMusic;
    
    // Storing sound effect audios for each action or interraction
    SoundEffect brickSnd;
    SoundEffect questionSnd;
    SoundEffect pipeSnd;
    SoundEffect scoreSnd;
    SoundEffect pauseSnd;
    SoundEffect starSnd;
    
    // Storing default sound volume
    float SND_VOLUME = 0.6f;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Attempting to set screen dimensions to 1600x1000
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 1000;
        
        // Applying the screen dimensions changes
        _graphics.ApplyChanges();

        // Storing the resultant dimensions to determine the drawable space
        screenWidth = _graphics.GraphicsDevice.Viewport.Width;
        screenHeight = _graphics.GraphicsDevice.Viewport.Height;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Loading sprite fonts
        smallFont = Content.Load<SpriteFont>("Fonts/SmallText");
        mediumFont = Content.Load<SpriteFont>("Fonts/MedText");
        largeFont = Content.Load<SpriteFont>("Fonts/LargeText");
        
        // Loading title image and rectangle
        titleImg = Content.Load<Texture2D>("Images/Sprites/Title");
        titleRec = new Rectangle((int)WidthCenter(titleImg.Width), 0, titleImg.Width, titleImg.Height);
        
        // Loading PRESS ENTER position
        enterPos = new Vector2(WidthCenter(mediumFont.MeasureString(ENTER_TEXT).X), screenHeight - 220);
        
        // Loading press enter menu position and space enter menu position
        enterMenuPos = new Vector2(WidthCenter(mediumFont.MeasureString(ENTER_TEXT_MENU).X), screenHeight - 200);
        spaceMenuPos = new Vector2(enterMenuPos.X, enterMenuPos.Y - 200);
        
        // Loading PAUSE title position
        pausePos = new Vector2(WidthCenter(largeFont.MeasureString(PAUSE_TITLE).X),
                            HeightCenter(largeFont.MeasureString(PAUSE_TITLE).Y));
        
        // Loading end game title position
        endgamePos = new Vector2(WidthCenter(largeFont.MeasureString(ENDGAME_TITLE).X), 15);
        
        // Loading score positions
        scoreLeftPos = new Vector2(5, 5);
        scoreRightPos = new Vector2(screenWidth - mediumFont.MeasureString("11").X, 5);
        
        // Loading game timer position of display
        gameTimerPos = new Vector2(WidthCenter(mediumFont.MeasureString("11").X), 5);
        
        // Loading brick texture
        brickImg = Content.Load<Texture2D>("Images/Sprites/Brick");
        
        // Loading location for each top and bottom brick
        for (int i = 0; i <= topBrickRecs.Length - 1; i++)
        {
            topBrickRecs[i] = new Rectangle(100 * i, 100, 100, 100);
            bottomBrickRecs[i] = new Rectangle(100 * i, screenHeight - 100, 100, 100);
        }
        
        // Loading question block texture
        questionImg = Content.Load<Texture2D>("Images/Sprites/Question");
        
        // Loading location for each left and right question block
        for (int i = 0; i <= leftQuestRecs.Length - 1; i++)
        {
            // Loading rectangles
            leftQuestRecs[i] = new Rectangle(0, 100 * (i + 2), 100, 100);
            rightQuestRecs[i] = new Rectangle(screenWidth - 100, 100 * (i + 2), 100, 100);
            
            // Loading initial colors
            leftQuestColor[i] = Color.White;
            rightQuestColor[i] = Color.White;
        }
        
        // Loading pipe textures
        leftPipeImg = Content.Load<Texture2D>("Images/Sprites/PipeV_L");
        rightPipeImg = Content.Load<Texture2D>("Images/Sprites/PipeV_R");
        
        // Loading shell texture
        shellImg = Content.Load<Texture2D>("Images/Sprites/RedShell");
        
        // Loading pipe positions and rectangles
        pipeRecs[0] = new Rectangle(250, leftQuestRecs[3].Center.Y - leftPipeImg.Height, 
            leftPipeImg.Width * 2, leftPipeImg.Height * 2);
        pipeRecs[1] = new Rectangle(screenWidth - 250 - rightPipeImg.Width * 2, 
            leftQuestRecs[3].Center.Y - leftPipeImg.Height, 
            rightPipeImg.Width * 2, rightPipeImg.Height * 2);
                    
        // Load shell position and rectangle
        shellRec = new Rectangle((int)WidthCenter(100f), leftQuestRecs[3].Center.Y - 50, 100, 100);
        shellPos = shellRec.Location.ToVector2();
        
        // Loading position for winner display
        winnerPos = new Vector2(0, endgamePos.Y + largeFont.MeasureString(ENDGAME_TITLE).Y);
        
        // Loading positions for statistics
        rightStatsPos = new Vector2(0, HeightCenter(smallFont.MeasureString("1").Y));
        leftStatsPos = new Vector2(0, rightStatsPos.Y - smallFont.MeasureString("1").Y - 20);
        gamesPlayedPos = new Vector2(0, rightStatsPos.Y + smallFont.MeasureString("1").Y + 20);
        
        // Loading coin image and animation
        coinImg = Content.Load<Texture2D>("Images/Sprites/CoinHDsm");
        coinLeftPos = new Vector2(WidthCenter(coinImg.Width / 8f) - screenWidth / 4f, 0);
        coinRightPos = new Vector2(WidthCenter(coinImg.Width / 8f) + screenWidth / 4f, 0);
        coinLeftAnim = new Animation(coinImg, 8, 4, 30, 0, -1, 1, 1000, coinLeftPos, false);
        coinRightAnim = new Animation(coinImg, 8, 4, 30, 0, -1, 1, 1000, coinRightPos, false);
        
        // Loading star image, position, animation
        starImg = Content.Load<Texture2D>("Images/Sprites/RotatingStar");
        starPos = shellPos;
        starAnim = new Animation(starImg, 5, 5, 25, 0, -1, 3, 1000, starPos, false);
        
        // Loading music for each game state
        menuMusic = Content.Load<Song>("Audio/Music/Menu");
        playMusic = Content.Load<Song>("Audio/Music/Gameplay");
        pauseMusic = Content.Load<Song>("Audio/Music/Pause");
        endgameMusic = Content.Load<Song>("Audio/Music/EndGame");
        
        // Loading sounds
        brickSnd = Content.Load<SoundEffect>("Audio/Sounds/BrickBlock");
        questionSnd = Content.Load<SoundEffect>("Audio/Sounds/QuestionBlock");
        pipeSnd = Content.Load<SoundEffect>("Audio/Sounds/PipeHit");
        scoreSnd = Content.Load<SoundEffect>("Audio/Sounds/Score");
        pauseSnd = Content.Load<SoundEffect>("Audio/Sounds/PauseSnd");
        starSnd = Content.Load<SoundEffect>("Audio/Sounds/StarCollect");
        
        // Playing menu music
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.9f;
        MediaPlayer.Play(menuMusic);
    }

    protected override void Update(GameTime gameTime)
    {
        // Updating keyboard state
        prevKb = kb;
        kb = Keyboard.GetState();
        
        // Updating the game logic based on the game state
        switch (gameState)
        {
            case MENU:
                // Checking for enter click (see more in method)
                EnterClick();
                
                // Checking for space click to play AI
                SpaceClick();
                
                break;
            
            case PRE_LAUNCH:
                // Checking for enter click (see more in method)
                EnterClick();
                
                break;
            
            case PLAY:
                // Updating game timer and storing time left in seconds
                gameTimer.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                timeLeftDisp = $"{gameTimer.GetTimeRemainingInt() / 1000}";
                timeLeftDisp = timeLeftDisp.PadLeft(2, '0');
                
                // Updating coin animation
                coinLeftAnim.Update(gameTime);
                coinRightAnim.Update(gameTime);
                
                // Translating pipes
                TranslatePipes();
                
                // Translating shells
                TranslateShell();
                
                // Rotation star logic
                StarBuffLogic(gameTime);
                
                // Checking for any collision (see more in method)
                Collisions();

                // Checking for enter click (see more in method)
                EnterClick();

                // Checking for game over conditions and determining winner 
                ChooseWinner();
                
                break;
            
            case PAUSE:
                // Checking for enter click (see more in method)
                EnterClick();
                
                break;
            
            case ENDGAME:
                // Checking for enter click (see more in method)
                EnterClick();
                
                break;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.RoyalBlue);
        
        // Initializing sprite drawing batch
        _spriteBatch.Begin();

        // Updating the drawing logic based on the game state
        switch (gameState)
        {
            case MENU:
                // Drawing main title
                _spriteBatch.Draw(titleImg, titleRec, Color.White);
                
                // Drawing Enter Prompt
                _spriteBatch.DrawString(mediumFont, ENTER_TEXT_MENU, enterMenuPos + SHADOW_OFFSET, Color.Black);
                _spriteBatch.DrawString(mediumFont, ENTER_TEXT_MENU, enterMenuPos, Color.White);
                
                // Drawing Space Prompt
                _spriteBatch.DrawString(mediumFont, SPACE_TEXT_MENU, spaceMenuPos + SHADOW_OFFSET, Color.Black);
                _spriteBatch.DrawString(mediumFont, SPACE_TEXT_MENU, spaceMenuPos, Color.White);
                
                break;
            
            case PRE_LAUNCH:
                // Drawing everything related to the game
                GameDraw();
                
                // Drawing Enter prompt
                EnterDraw(Color.Red, Color.White);
                break;
            
            case PLAY:
                // Drawing everything related to the game
                GameDraw();
                
                break;
            
            case PAUSE:
                // Changing background color to white
                GraphicsDevice.Clear(Color.Black);
                
                // Drawing paused title
                _spriteBatch.DrawString(largeFont, PAUSE_TITLE, pausePos + SHADOW_OFFSET, Color.White);
                _spriteBatch.DrawString(largeFont, PAUSE_TITLE, pausePos, Color.Blue);
                
                // Drawing Enter prompt
                EnterDraw(Color.Red, Color.White);
                
                break;
            
            case ENDGAME:
                // Drawing title
                _spriteBatch.DrawString(largeFont, ENDGAME_TITLE, endgamePos + SHADOW_OFFSET, Color.Black);
                _spriteBatch.DrawString(largeFont, ENDGAME_TITLE, endgamePos, Color.Red);
                
                // Drawing winner display:
                _spriteBatch.DrawString(mediumFont, winnerDisp, winnerPos + SHADOW_OFFSET, Color.Black);
                _spriteBatch.DrawString(mediumFont, winnerDisp, winnerPos, Color.Yellow);
                
                // Drawing statistics:
                DrawStats();
                
                // Drawing Enter prompt
                EnterDraw(Color.White, Color.Black);
                break;
        }
        
        // Finish sprite batch
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    // The function uses screen width and text/image width to center the image horizontally
    private float WidthCenter(float spriteWidth)
    {
        float center;

        // Calculating center by averaging the 2 widths
        center = (screenWidth - spriteWidth) / 2f;

        return center;
    }
    
    // The function uses screen height and text/image height to center the image vertically
    private float HeightCenter(float spriteHeight)
    {
        float center;

        // Calculating center by averaging the 2 heights
        center = (screenHeight - spriteHeight) / 2f;

        return center;
    }
    
    // Drawing Enter Prompt based on color and drop shadow color
    private void EnterDraw(Color color, Color shadowColor)
    {
        // Drawing enter prompt and drop shadow
        _spriteBatch.DrawString(mediumFont, ENTER_TEXT, enterPos + SHADOW_OFFSET, shadowColor);
        _spriteBatch.DrawString(mediumFont, ENTER_TEXT, enterPos, color);
    }
    
    // Updating game logic after Enter pressed
    private void EnterClick()
    {
        // Detecting if enter was pressed
        if (kb.IsKeyDown(Keys.Enter) && !prevKb.IsKeyDown(Keys.Enter))
        {
            // Updating game logic based on the game state
            switch (gameState)
            {
                case MENU:
                    // Setting game state to pre launch
                    gameState = PRE_LAUNCH;
                    
                    // Switching to game music
                    MediaPlayer.Stop();
                    MediaPlayer.Play(playMusic);
                    
                    // Setting number of players to 2
                    numPlayers = 2;
                    
                    // RESET GAME
                    ResetGame();
                    
                    break;
            
                case PRE_LAUNCH:
                    // Setting game state to play
                    gameState = PLAY;
                    
                    // START GAME
                    // Randomizing intial velocity
                    CalcVel();
                    
                    // Starting game timer and star timer
                    gameTimer.Activate();
                    starTimer.Activate();

                    break;
                
                case PLAY:
                    // Setting game state to pause
                    gameState = PAUSE;
                    
                    // Playing pause music
                    MediaPlayer.Stop();
                    MediaPlayer.Play(pauseMusic);
                    
                    // Playing pause sound
                    PlaySound(pauseSnd);
                    
                    break;
            
                case PAUSE:
                    // Setting game state to play
                    gameState = PLAY;
                    
                    // Playing gameplay music
                    MediaPlayer.Stop();
                    MediaPlayer.Play(playMusic);
                    
                    // Playing unpause sound
                    PlaySound(pauseSnd);
                    
                    break;
            
                case ENDGAME:
                    // Setting game state to menu
                    gameState = MENU;

                    // Playing pause music
                    MediaPlayer.Stop();
                    MediaPlayer.Play(menuMusic);
                    break;
            }
        }
    }
    
    // Checking for space click in menu for AI player
    private void SpaceClick()
    {
        if (kb.IsKeyDown(Keys.Space))
        {
            // Setting game state to pre launch
            gameState = PRE_LAUNCH;
                    
            // Setting number of players to 1
            numPlayers = 1;
                    
            // RESET GAME
            ResetGame();
            
            // Switching to game music
            MediaPlayer.Stop();
            MediaPlayer.Play(playMusic);
        }
    }

    // Resetting game before every game
    private void ResetGame()
    {
        // Resetting positions 
        ResetRound();

        // Resetting scores
        scoreLeft = 0;
        scoreRight = 0;
        
        // Resetting Game and star Timer
        gameTimer.ResetTimer(false);
        starTimer.ResetTimer(false);
        
        // Resetting state of each block
        for (int i = 0; i <= leftQuestRecs.Length - 1; i++)
        {
            // Loading initial colors
            leftQuestColor[i] = Color.White;
            rightQuestColor[i] = Color.White;
        }
        
        // Resetting last player to hit the shell
        lastShellHit = 2;
    }
    
    // Resetting the round after edge collision
    private void ResetRound()
    {
        // reset pipe position
        pipeRecs[0].X = 250;
        pipeRecs[0].Y = leftQuestRecs[3].Center.Y - leftPipeImg.Height;

        pipeRecs[1].X = screenWidth - 250 - rightPipeImg.Width * 2;
        pipeRecs[1].Y = pipeRecs[0].Y;
        
        // Resetting shell position and rectangle
        shellRec.X = (int)WidthCenter(100f);
        shellRec.Y = leftQuestRecs[3].Center.Y - 50;
        shellPos = shellRec.Location.ToVector2();
        
        // Setting game state to pre launch
        gameState = PRE_LAUNCH;
        
        // Resetting last player to hit the shell 
        lastShellHit = 2;
    }
    
    // All drawing related logic for pre launch and play
    private void GameDraw()
    {
        // Changing background color to white
        GraphicsDevice.Clear(Color.Black);
        
        // Drawing top and bottom bricks
        for (int i = 0; i <= topBrickRecs.Length - 1; i++)
        {
            _spriteBatch.Draw(brickImg, topBrickRecs[i], Color.White);
            _spriteBatch.Draw(brickImg, bottomBrickRecs[i], Color.White);
        }
        
        // Drawing left and right question bricks
        for (int i = 0; i <= leftQuestRecs.Length - 1; i++)
        {
            _spriteBatch.Draw(questionImg, leftQuestRecs[i], leftQuestColor[i]);
            _spriteBatch.Draw(questionImg, rightQuestRecs[i], rightQuestColor[i]);
        }
        
        // Drawing pipes
        _spriteBatch.Draw(leftPipeImg, pipeRecs[0], Color.White);
        _spriteBatch.Draw(rightPipeImg, pipeRecs[1], Color.White);
        
        // Drawing shell
        _spriteBatch.Draw(shellImg, shellRec, Color.White);
        
        // HUD
        // Drawing scores with drop shadow offset
        _spriteBatch.DrawString(mediumFont, scoreLeft.ToString().PadLeft(2, '0'), 
                        scoreLeftPos + SHADOW_OFFSET, Color.White);
        _spriteBatch.DrawString(mediumFont, scoreLeft.ToString().PadLeft(2, '0'), scoreLeftPos, Color.Blue);
        
        _spriteBatch.DrawString(mediumFont, scoreRight.ToString().PadLeft(2, '0'), 
                        scoreRightPos + SHADOW_OFFSET, Color.White);
        _spriteBatch.DrawString(mediumFont, scoreRight.ToString().PadLeft(2, '0'), scoreRightPos, Color.Blue);
        
        // Drawing timer with drop shadow
        _spriteBatch.DrawString(mediumFont, timeLeftDisp, gameTimerPos + SHADOW_OFFSET, Color.White);
        _spriteBatch.DrawString(mediumFont, timeLeftDisp, gameTimerPos , Color.Red);
        
        // Drawing spinning coin
        coinLeftAnim.Draw(_spriteBatch, Color.White); 
        coinRightAnim.Draw(_spriteBatch, Color.White); 
        
        // Drawing spinning star
        starAnim.Draw(_spriteBatch, Color.White);
    }
    
    # region METHODS RELATED TO TRANSLATION
    // Calclulating initial shell velocity
    private void CalcVel()
    {
        // Randomizing X and Y velocities
        shellVel.X = (float)(2 + rng.NextDouble() * 10);
        shellVel.Y = (float)(2 + rng.NextDouble() * 10);

        // Randomizing initial direction of shell for X and Y seperately
        if (rng.Next(1, 101) < 50)
        {
            // Inverting X velocity
            shellVel.X *= -1;
        }

        if (rng.Next(1, 101) < 50)
        {
            // Inverting Y velocity
            shellVel.Y *= -1;
        }
    }
    
    // Translating pipes
    private void TranslatePipes()
    {
        // Translating left pipe based on player input
        if (kb.IsKeyDown(Keys.W))
        {
            // Checking if pipe is in playing area
            if (pipeRecs[0].Y > topBrickRecs[0].Bottom)
            {
                // Translate pipe 0 up
                pipeRecs[0].Y -= PIPE_SPEED;
            }
        }
        if (kb.IsKeyDown(Keys.S))
        {
            // Checking if pipe is in playing area
            if (pipeRecs[0].Bottom < bottomBrickRecs[0].Top)
            {
                // Translate pipe 0 down
                pipeRecs[0].Y += PIPE_SPEED;
            }
        }

        // Determining who moves pipe 2 based on how many players selected
        if (numPlayers == 2)
        {
            // Translating right pipe based on player input
            if (kb.IsKeyDown(Keys.Up))
            {
                // Checking if pipe is in playing area
                if (pipeRecs[1].Y > topBrickRecs[0].Bottom)
                {
                    // Translate pipe 1 up
                    pipeRecs[1].Y -= PIPE_SPEED;
                }            
            }

            if (kb.IsKeyDown(Keys.Down))
            {
                // Checking if pipe is in playing area
                if (pipeRecs[1].Bottom < bottomBrickRecs[0].Top)
                {
                    // Translate pipe 1 down
                    pipeRecs[1].Y += PIPE_SPEED;
                }
            }
        }
        else
        {
            // tranlating right pipe using ai player
            AiPlayerTranslate();
        }
    }
    
    // Handling translation for AI player
    private void AiPlayerTranslate()
    {
        // Calculating destination position of the shell for AI player
        futureYShellPos = shellPos.Y + (pipeRecs[1].X - shellPos.X) * shellVel.Y / shellVel.X;
        
        // Translating pipe if shell is moving towards it 
        if (shellVel.X > 0)
        {
            // Moving pipe up towards the shell
            if (futureYShellPos < pipeRecs[1].Center.Y)
            {
                // Checking if pipe is in playing area
                if (pipeRecs[1].Y > topBrickRecs[0].Bottom)
                {
                    // Translate pipe 1 up
                    pipeRecs[1].Y -= PIPE_SPEED;
                }
            }
            else if (futureYShellPos > pipeRecs[1].Center.Y)
            {
                // Checking if pipe is in playing area
                if (pipeRecs[1].Bottom < bottomBrickRecs[0].Top)
                {
                    // Translate pipe 1 down
                    pipeRecs[1].Y += PIPE_SPEED;
                }
            }
        }
    }
    
    // Translate shell
    private void TranslateShell()
    {
        // Adding speed to shell position
        shellPos += shellVel;
        
        // Setting shell rectangle position to shellPos but integer
        shellRec.Location = shellPos.ToPoint();
    }
    #endregion

    #region METHODS HANDLING COLLISIONS:

    // Main centeral collision method:
    private void Collisions()
    {
        // Updating mid points
        UpdateMidPoints();
        
        // Checking for brick collision
        BrickCollision();

        // Checking for pipe collision
        PipeCollision();
        
        // Checking for question collision
        QuestionCollision();

        // Checking for edge collision
        EdgeCollision();
    }
    
    // Updating mid points:
    private void UpdateMidPoints()
    {
        // Updating mid points
        shellMidTop.X = shellRec.Center.X;
        shellMidTop.Y = shellRec.Top;
        
        shellLeftMid.X = shellRec.Left; 
        shellLeftMid.Y = shellRec.Center.Y;

        shellRightMid.X = shellRec.Right;
        shellRightMid.Y = shellRec.Center.Y;

        shellMidBottom.X = shellRec.Center.X;
        shellMidBottom.Y = shellRec.Bottom;
    }

    private float QuestMidPointLeft(float speed, Vector2 midPoint, int i)
    {
        if (leftQuestRecs[i].Contains(midPoint))
        {
            // Speed up shell
            speed = SpeedUpInvert(speed);
                    
            // Set color to be transparent
            leftQuestColor[i] = Color.White * 0.6f;
                    
            // Adding 1 point to opposite player score
            scoreRight++;
                    
            // Activating coin animation
            coinRightAnim.Activate(true);
            
            // Playing question collision sound
            PlaySound(questionSnd);
        }

        return speed;
    }
    
    private float QuestMidPointRight(float speed, Vector2 midPoint, int i)
    {
        if (rightQuestRecs[i].Contains(midPoint))
        {
            // Speed up shell
            speed = SpeedUpInvert(speed);
                    
            // Set color to be transparent
            rightQuestColor[i] = Color.White * 0.6f;
                    
            // Adding 1 point to opposite player score
            scoreLeft++;
                    
            // Activating coin animation
            coinRightAnim.Activate(true);
            
            // Playing question collision sound
            PlaySound(questionSnd);
        }

        return speed;
    }
    
    // Speeding up chosen velocity component
    private float SpeedUpInvert(float speed)
    {
        // Invert speed
        speed *= -1;
        
        // Increasing the magnitude of the X velocity with respect to the direction
        if (speed > 0)
            speed++;
        else
            speed--;
        
        // Clamping X velocity
        speed = Math.Clamp(speed, -15, 15);

        // Returning modified speed
        return speed;
    }
    
    // Collision with bricks
    private void BrickCollision()
    {
        // Check if shell hit top or bottom bricks
        if (shellRec.Top < topBrickRecs[0].Bottom)
        {
            // Speed up shell
            shellVel.Y = SpeedUpInvert(shellVel.Y);

            // Resetting shell position
            shellPos.Y = topBrickRecs[0].Bottom;
            
            // Playing brick collision sound
            PlaySound(brickSnd);
        }
        else if (shellRec.Bottom > bottomBrickRecs[0].Top)
        {
            // Speed up shell
            shellVel.Y = SpeedUpInvert(shellVel.Y);
            
            // Resetting shell position
            shellPos.Y = bottomBrickRecs[0].Top - shellRec.Height;
            
            // Playing brick collision sound
            PlaySound(brickSnd);
        }
    }

    // Collision with pipes
    private void PipeCollision()
    {
        // Handling collisions for each pipe
        for (int i = 0; i < pipeRecs.Length; i++)
        {
            // Handling collision for each midpoint
            if (pipeRecs[i].Contains(shellMidTop.ToPoint()))
            {
                // Moving shell out of pipe
                shellPos.Y = pipeRecs[i].Bottom;
                
                // Speed up shell Y velocity
                shellVel.Y = SpeedUpInvert(shellVel.Y);
                
                // Storing last pipe to hit shell
                lastShellHit = (byte)i;
                
                // Playing pipe hit sound
                PlaySound(pipeSnd);
            }

            else if (pipeRecs[i].Contains(shellMidBottom.ToPoint()))
            {
                // Moving shell out of pipe
                shellPos.Y = pipeRecs[i].Top - shellRec.Height;
                
                // Speed up shell Y velocity
                shellVel.Y = SpeedUpInvert(shellVel.Y);
                
                // Storing last pipe to hit shell
                lastShellHit = (byte)i;
                
                // Playing pipe hit sound
                PlaySound(pipeSnd);
            }

            else if (pipeRecs[i].Contains(shellLeftMid.ToPoint()))
            {
                // Moving shell out of pipe
                shellPos.X = pipeRecs[i].Right;
                
                // Speed up shell
                shellVel.X = SpeedUpInvert(shellVel.X);
                
                // Storing last pipe to hit shell
                lastShellHit = (byte)i;
                
                // Playing pipe hit sound
                PlaySound(pipeSnd);
            }

            else if (pipeRecs[i].Contains(shellRightMid.ToPoint()))
            {
                // Moving shell out of pipe
                shellPos.X = pipeRecs[i].Left - shellRec.Width;
                
                // Speed up shell
                shellVel.X = SpeedUpInvert(shellVel.X);
                
                // Storing last pipe to hit shell
                lastShellHit = (byte)i;
                
                // Playing pipe hit sound
                PlaySound(pipeSnd);
            }
        }
    }
    
    // Collision with question blocks
    private void QuestionCollision()
    {
        // Doing collision for each block seperately
        for (int i = 0; i <= leftQuestRecs.Length - 1; i++)
        {
            // Left side collisions
            if (leftQuestColor[i] == Color.White)
            {
                // Checking collision for each midpoint (see method for more)
                shellVel.X = QuestMidPointLeft(shellVel.X, shellLeftMid, i);
                
                shellVel.Y = QuestMidPointLeft(shellVel.Y, shellMidTop, i);
                
                shellVel.Y = QuestMidPointLeft(shellVel.Y, shellMidBottom, i);
            }
            // Right side collisions
            if (rightQuestColor[i] == Color.White)
            {
                // Checking collision for each midpoint (see method for more)
                shellVel.X = QuestMidPointRight(shellVel.X, shellRightMid, i);
                
                shellVel.Y = QuestMidPointRight(shellVel.Y, shellMidTop, i);
                
                shellVel.Y = QuestMidPointRight(shellVel.Y, shellMidBottom, i);
            }
        }
    }

    // Collision with edges
    private void EdgeCollision()
    {
        if (shellRec.Right > screenWidth)
        {
            // Reset round
            ResetRound();
            
            // Adding scores right player
            scoreLeft += 2;
            
            // Playing score point sound
            PlaySound(scoreSnd);
        }
        
        if (shellRec.Left < 0)
        {
            // Reset round
            ResetRound();
            
            // Adding scores to left player
            scoreRight += 2;
            
            // Playing score point sound
            PlaySound(scoreSnd);
        }
    }

    #endregion 
    
    // Choosing winner based on score when game is over
    private void ChooseWinner()
    {
        if (scoreLeft >= 15)
        {
            // Setting winner to left player (player 1)
            GameOver("PLAYER 1 WINS!");
            
            // Increasing 1 win to left player
            winsLeft++;

            // Updating stats display
            StatsDisplay();
        }
        else if (scoreRight >= 15)
        {
            // Setting winner to right player (player 2)
            GameOver("PLAYER 2 WINS!");
            
            // Increasing 1 win to right player
            winsRight++;
            
            // Updating stats display
            StatsDisplay();
        }
        else if (gameTimer.IsFinished())
        {
            // Determining who won and updating winner text to match
            if (scoreLeft > scoreRight)
            {
                GameOver("PLAYER 1 WINS!");
                
                // Increasing 1 win to left player
                winsLeft++;
            }
            else if (scoreRight > scoreLeft)
            {
                GameOver("PLAYER 2 WINS!");
                
                // Increasing 1 win to right player
                winsRight++;
            }
            else
            {
                GameOver("IT IS A TIE!");
            }
            
            // Updating stats display
            StatsDisplay();
        }
    }
    // Logic for displaying winner
    private void GameOver(string winnerText)
    {
        // Setting game state to end game
        gameState = ENDGAME;

        // Storing winning player
        winnerDisp = winnerText;
            
        // Updating location of display
        winnerPos.X = WidthCenter(mediumFont.MeasureString(winnerDisp).X);
        
        // Adding 1 to games played
        gamesPlayed++;
        
        // Playing end game music
        MediaPlayer.Stop();
        MediaPlayer.Play(endgameMusic);
    }
    
    // Updating statistic display positions
    private void StatsDisplay()
    {            
        // Calculating win %
        winPercentLeft = (float)winsLeft / gamesPlayed * 100f;
        winPercentRight = (float)winsRight / gamesPlayed * 100f;
        
        // Updating stats displays
        leftStatsDisp = $"Player 1 Wins: {winsLeft} - Win %: {Math.Round(winPercentLeft, 0)}";
        rightStatsDisp = $"Player 2 Wins: {winsRight} - Win %: {Math.Round(winPercentRight, 0)}";
        gamesPlayedDisp = $"Games Played: {gamesPlayed}";
        
        // Updating stat display positions
        leftStatsPos.X = WidthCenter(smallFont.MeasureString(leftStatsDisp).X);
        rightStatsPos.X = WidthCenter(smallFont.MeasureString(rightStatsDisp).X);
        gamesPlayedPos.X = WidthCenter(smallFont.MeasureString(gamesPlayedDisp).X);
    }
    
    // Drawing statistics
    private void DrawStats()
    {
        // Drawing left player stats with drop shadow
        _spriteBatch.DrawString(smallFont, leftStatsDisp, leftStatsPos + SHADOW_OFFSET, Color.Black);
        _spriteBatch.DrawString(smallFont, leftStatsDisp, leftStatsPos, Color.SeaGreen);
        
        // Drawing right player stats with drop shadow
        _spriteBatch.DrawString(smallFont, rightStatsDisp, rightStatsPos + SHADOW_OFFSET, Color.Black);
        _spriteBatch.DrawString(smallFont, rightStatsDisp, rightStatsPos, Color.SeaGreen);
        
        // Drawing games played with drop shadow
        _spriteBatch.DrawString(smallFont, gamesPlayedDisp, gamesPlayedPos + SHADOW_OFFSET, Color.Black);
        _spriteBatch.DrawString(smallFont, gamesPlayedDisp, gamesPlayedPos, Color.LightPink);
        
    }
    
    // Handling all update logic for rotating star
    private void StarBuffLogic(GameTime gameTime)
    {
        // Updating star animation and timer
        starAnim.Update(gameTime);
        starTimer.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);

        // Playing star animation every 10 seconds
        if (starTimer.IsFinished())
        {
            starAnim.Activate(true);
            
            starTimer.ResetTimer(true);
        }
        
        // While star animation is active check for collisions with midpoints
        if (starAnim.IsAnimating())
        {
            if (starAnim.GetDestRec().Intersects(shellRec))
            {
                starAnim.Deactivate();

                if (lastShellHit == 0)
                {
                    scoreLeft += 2;
                }
                else if (lastShellHit == 1)
                {
                    scoreRight += 2;
                }
                
                // Playing star collect sound
                PlaySound(starSnd);
            }
        }
    }
    
    // Creating sound instance
    private void PlaySound(SoundEffect sound)
    {
        // Playing chosen sound at default volume
        SoundEffectInstance snd = sound.CreateInstance();
        snd.Volume = SND_VOLUME;
        snd.Play();
    }
}
