using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace LD49
{
    public class LD49Game : Game
    {
#if DEBUG
        public const string LevelsDir = @"..\..\..\Content\Levels";
#else
        public const string LevelsDir = @".\Content\Levels";
#endif

        private GraphicsDeviceManager _graphics;
        private ContentLibrary _contentLibrary;
        private SpriteBatch _sb;
        private GameState _gameState;
        private KeyboardState _lastKB;
        private KeyboardState _currKB;
        private MouseState _lastMS;
        private MouseState _currMS;
        private bool _editMode = false;

        private int _tileEditToolIdx = 0;
        private readonly char[] _tileEditTools = new char[] { 'W', 'T', 'I', 'L', 'X' };
        private byte _tileEditRotation = 0;

        public LD49Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            _gameState = new GameState();
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1440;
            _graphics.PreferredBackBufferHeight = 900;
            _graphics.ApplyChanges();
            base.Initialize();
            _sb = new SpriteBatch(GraphicsDevice);
            _gameState.Start();
        }

        protected override void LoadContent()
        {
            _contentLibrary = new ContentLibrary(
                Content.LoadTexture2Ds(),
                Content.LoadSpriteFonts()
            );

            var levelFiles = Directory.GetFiles(LevelsDir);
            foreach (var file in levelFiles)
                LoadLevelFromFile(file);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _lastKB = _currKB;
            _currKB = Keyboard.GetState();

            _lastMS = _currMS;
            _currMS = Mouse.GetState();

            if (KeyWasPressed(Keys.E)) { _editMode = !_editMode; }

            if (_editMode)
            {
                if (KeyWasPressed(Keys.Tab)) { _tileEditToolIdx++; _tileEditToolIdx = _tileEditToolIdx >= _tileEditTools.Length ? 0 : _tileEditToolIdx; }
                if (MouseWheelUp) { _tileEditRotation++; _tileEditRotation = _tileEditRotation >= 4 ? (byte)0 : _tileEditRotation; }
                if (MouseWheelDown) { _tileEditRotation--; _tileEditRotation = _tileEditRotation < 0 ? (byte)4 : _tileEditRotation; }
                if (RightClicked)
                {
                    var targetTilePos = MouseTilePos;
                    if (_gameState.ActiveLevel.State.ContainsKey(targetTilePos))
                        _gameState.ActiveLevel.State.Remove(targetTilePos);
                }
                else if (LeftClicked)
                {
                    var targetTilePos = MouseTilePos;
                    if (!_gameState.ActiveLevel.State.ContainsKey(targetTilePos))
                    {
                        _gameState.ActiveLevel.State.Add(targetTilePos, new Tile());
                    }

                    _gameState.ActiveLevel.State[targetTilePos].Type = _tileEditTools[_tileEditToolIdx];
                    _gameState.ActiveLevel.State[targetTilePos].Rotation = _tileEditRotation;
                    _gameState.ActiveLevel.State[targetTilePos].Occupant = '0';
                }
                else if (MiddleClicked)
                {
                    var targetTilePos = MouseTilePos;
                    if (_gameState.ActiveLevel.State.ContainsKey(targetTilePos))
                        _gameState.ActiveLevel.State[targetTilePos].IsFixed = !_gameState.ActiveLevel.State[targetTilePos].IsFixed;
                }
                else if (KeyWasPressed(Keys.S))
                {
                    var levelFilePath = Path.Combine(LevelsDir, $"{_gameState.ActiveLevel.Number}.txt");
                    var levelData = _gameState.ReadLevel(_gameState.ActiveLevel.Number);
                    File.WriteAllLines(levelFilePath, levelData);
                }
                else if (KeyWasPressed(Keys.R))
                {
                    var levelFilePath = Path.Combine(LevelsDir, $"{_gameState.ActiveLevel.Number}.txt");
                    LoadLevelFromFile(levelFilePath);
                }
                else if (KeyWasPressed(Keys.C))
                {
                    _gameState.ActiveLevel.State.Clear();
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _sb.Begin();

            var tilesTexture = _contentLibrary.Textures[Texture2Ds.tiles];

            foreach (var tilePosition in _gameState.ActiveLevel.State.Keys)
            {
                var tile = _gameState.ActiveLevel.State[tilePosition];
                if (tile.Type == Tile.EmptyValue && !_editMode) continue;
                var sourceRect = GetSourceRectableForTileType(tile.Type);
                var position = tilePosition.ToVector2() * (TileDimensions + TileOffset);
                var rotation = MathHelper.ToRadians(tile.Rotation * 90f);
                _sb.Draw(
                    tilesTexture,
                    position + TileOrigin,
                    sourceRect,
                    tile.IsFixed ? Color.Gray : Color.White,
                    rotation,
                    TileOrigin,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            _sb.End();

            if (!_editMode) return;

            _sb.Begin();
            var eSourceRect = GetSourceRectableForTileType(_tileEditTools[_tileEditToolIdx]);
            var eRotation = MathHelper.ToRadians(_tileEditRotation * 90f);
            _sb.Draw(
                tilesTexture,
                (MouseTilePos.ToVector2() * (TileDimensions + TileOffset)) + TileOrigin,
                eSourceRect,
                Color.White,
                eRotation,
                TileOrigin,
                1f,
                SpriteEffects.None,
                0f
            );
            _sb.End();
        }        

        private const int TileSize = 110;
        private const int TileBuffer = 4;
        public static readonly Vector2 TileOffset = new Vector2(TileBuffer, TileBuffer);
        public static readonly Vector2 TileDimensions = new Vector2(TileSize, TileSize);
        public static readonly Vector2 TileOrigin = new Vector2(TileSize * 0.5f, TileSize * 0.5f);

        private Rectangle GetSourceRectableForTileType(char type)
        {
            var rectPoint = new Point(0, 0);
            var rectSize = TileDimensions.ToPoint();
            var offset = new Vector2(TileSize + TileBuffer, 0);
            return type switch
            {
                'W' => new Rectangle(rectPoint, rectSize),
                'T' => new Rectangle(rectPoint + offset.ToPoint(), rectSize),
                'X' => new Rectangle(rectPoint + (offset * 2).ToPoint(), rectSize),
                'L' => new Rectangle(rectPoint + (offset * 3).ToPoint(), rectSize),
                'I' => new Rectangle(rectPoint + (offset * 4).ToPoint(), rectSize),
                _ => new Rectangle(rectPoint + (offset * 6).ToPoint(), rectSize),
            };
        }

        private void LoadLevelFromFile(string file)
        {
            var levelNumber = int.Parse(Path.GetFileNameWithoutExtension(file));
            var levelData = File.ReadAllLines(file);
            _gameState.LoadLevel(levelNumber, levelData);
        }

        private bool KeyWasPressed(Keys e) => _currKB.IsKeyUp(e) && _lastKB.IsKeyDown(e);
        private bool RightClicked => _currMS.RightButton == ButtonState.Released && _lastMS.RightButton == ButtonState.Pressed;
        private bool MiddleClicked => _currMS.MiddleButton == ButtonState.Released && _lastMS.MiddleButton == ButtonState.Pressed;
        public bool LeftClicked => _currMS.LeftButton == ButtonState.Released && _lastMS.LeftButton == ButtonState.Pressed;
        public bool MouseWheelUp => _currMS.ScrollWheelValue < _lastMS.ScrollWheelValue;
        public bool MouseWheelDown => _currMS.ScrollWheelValue > _lastMS.ScrollWheelValue;
        private Point MouseTilePos => ((_currMS.Position.ToVector2()) / (TileDimensions + TileOffset)).ToPoint();
    }
}
