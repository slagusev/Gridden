﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Gridden
{
    /// <summary>
    /// Main form of the application.
    /// </summary>
    public partial class GridForm : Form
    {
        private MapEditor _mapEditor = MapEditor.Instance;
        private SheetEditor _sheetEditor = SheetEditor.Instance;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GridForm()
        {
            InitializeComponent();

            // an empty map.
            _mapEditor.CurrentMap = MapFactory.BuildNew();

            // load the sheet
            _sheetEditor.CurrentSheet = SheetFactory.LoadSheetFromFile();

            // setup form components
            RefreshDisplay();
        }

        #region Properties

        private int GridWidth
        {
            get
            {
                return _mapEditor.CurrentMap.MapWidth;
            }
        }

        private int GridHeight
        {
            get
            {
                return _mapEditor.CurrentMap.MapHeight;
            }
        }

        #endregion

        #region Menu event handlers

        // Map -> New
        private void newToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            MenuCommands.SetNewMap();
            RefreshDisplay();
        }

        // Map -> Save
        private void saveToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            string fileName = MenuCommands.SaveCurrentMapToFile();
            SetFormStatusText(String.Format("Saved successfully as {0}!", fileName));
        }

        // Map -> Load
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // loadedMap is null if user clicks "Cancel" in open dialog.
            Map loadedMap = MenuCommands.LoadMapFromFile();
            if (loadedMap != null)
            {
                _mapEditor.CurrentMap = loadedMap;
                RefreshDisplay();
            }
        }

        // Map -> Add Image
        private void addImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuCommands.AddImageToSheet();
            RefreshDisplay();
        }

        // Map -> Map Properties
        private void mapPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuCommands.ViewMapProperties();
            RefreshDisplay();
        }

        // Map -> View Text
        private void viewTextToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            MenuCommands.ViewMapText();
        }

        #endregion

        #region Mouse event handlers

        /// <summary>
        /// Event handler for clicking on the sheet "toolbar".
        /// Left-clicking is used to select a sprite and right-clicking is used to remove it from the sheet.
        /// </summary>
        private void spritePanel_MouseClick(object sender, MouseEventArgs e)
        {
            var sprites = _sheetEditor.GetSpritesFromSheet();

            // set the selected sprite to the one that was clicked on.
            for (int i = 0; i < sprites.Count; i++)
            {
                int baseX = 2;
                int baseY = 2;
                if (e.X > baseX + (i * (Sprite.Size + 2)) && e.X < baseX + (Sprite.Size + (i * (Sprite.Size + 2))) && e.Y > baseY && e.Y < baseY + Sprite.Size)
                {
                    _sheetEditor.SelectedSpriteIndex = i;
                    this.spritePanel.Invalidate();
                }
            }

            // if right-clicking, prompt the user to delete the selected sprite from the sheet
            if (e.Button == MouseButtons.Right)
            {
                int index = _sheetEditor.SelectedSpriteIndex;

                _mapEditor.RemoveCharacterFromMap(_sheetEditor.GetSpriteByIndex(index).Char);
                RefreshDisplay();

                _sheetEditor.RemoveSpriteFromCurrentSheet(index);
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Event handler for clicking on the paint panel (the grid).
        /// Left clicking is used to place tiles and right-clicking is used to remove them.
        /// </summary>
        private void paintPanel_Click(object sender, MouseEventArgs e)
        {
            if (_sheetEditor.SelectedSpriteIndex >= 0)
            {
                int tileX = e.X / Map.TileSize;
                int tileY = e.Y / Map.TileSize;

                if (e.Button == MouseButtons.Left)
                {
                    char c = _sheetEditor.GetSpritesFromSheet().Where(r => r.Index == _sheetEditor.SelectedSpriteIndex).First().Char;
                    _mapEditor.SetMapTile(tileX, tileY, c);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (!_mapEditor.IsMapPositionFree(tileX, tileY))
                    {
                        _mapEditor.ClearMapTile(tileX, tileY);
                    }
                }

                RefreshDisplay();
            }
        }

        #endregion

        #region Paint event handlers

        private void spritePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            List<Sprite> sprites = _sheetEditor.GetSpritesFromSheet();

            if (sprites.Count > 0)
            {
                // draw the tile selector.
                SolidBrush brush = new SolidBrush(Color.Blue);
                Rectangle r = new Rectangle(_sheetEditor.SelectedSpriteIndex * (Sprite.Size + 2), 0, Sprite.Size + 4, Sprite.Size + 4);
                g.FillRectangle(brush, r);

                int baseX = 2;
                int baseY = 2;
                for (int i = 0; i < sprites.Count; i++)
                {
                    // set scale and opacity.
                    Image img = ImageUtility.ScaleImage(sprites[i].Image, Sprite.Size, Sprite.Size);
                    if (i != _sheetEditor.SelectedSpriteIndex)
                    {
                        img = ImageUtility.SetImageOpacity(img, 0.5f);
                    }

                    g.DrawImage(img, new Point(baseX + i * (Sprite.Size + 2), baseY));
                }
            }
        }

        private void paintPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            // draw grid lines
            Pen p = new Pen(Color.Black);
            for (int i = 0; i < GridWidth; i++)
            {
                g.DrawLine(p, new Point(i * Map.TileSize, 0), new Point(i * Map.TileSize, Map.TileSize * GridHeight));
                for (int j = 0; j < GridHeight; j++)
                {
                    g.DrawLine(p, new Point(0, j * Map.TileSize), new Point(Map.TileSize * GridWidth, j * Map.TileSize));
                }
            }

            // draw the map
            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {

                    if (!_mapEditor.IsMapPositionFree(i, j))
                    {
                        char c = _mapEditor.GetMapCharAtPosition(i, j);
                        g.DrawImage(ImageUtility.ScaleImage(_sheetEditor.GetImageForCharacter(c), Map.TileSize, Map.TileSize), i * Map.TileSize, j * Map.TileSize);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Refresh the display by updating the form title and redrawing the sheet grid.
        /// </summary>
        private void RefreshDisplay()
        {
            Map currentMap = MapEditor.Instance.CurrentMap;

            // update the form title.
            this.Text = "Gridden: " + currentMap.Name;

            // resize and redraw the map editor.
            this.paintPanel.Size = new Size(currentMap.MapWidth * Map.TileSize, currentMap.MapHeight * Map.TileSize);
            this.paintPanel.Invalidate();

            this.spritePanel.Invalidate();
        }
    
        /// <summary>
        /// Sets the text in the form's status strip.
        /// </summary>
        public void SetFormStatusText(string text)
        {
            this.toolStripStatusLabel.Text = text;
            System.Timers.Timer t = new System.Timers.Timer(3000);
            t.Elapsed += statusTextTimer_Elapsed;
            t.Start();
        }

        void statusTextTimer_Elapsed(object sender, EventArgs e)
        {
            this.toolStripStatusLabel.Text = "";
        }
    }
}
