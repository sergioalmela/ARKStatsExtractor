﻿using ARKBreedingStats.species;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ARKBreedingStats.Library;
using ARKBreedingStats.utils;

namespace ARKBreedingStats.uiControls
{
    public partial class RegionColorChooser : UserControl
    {
        public event Action RegionColorChosen;
        private readonly Panel[] _panels;
        private readonly Button[] _buttonColors;
        private int[] _selectedRegionColorIds;
        public readonly bool[] ColorRegionsUseds;
        private readonly MyColorPicker _colorPicker;
        private List<ColorRegion> _colorRegions;
        private readonly ToolTip _tt = new ToolTip();
        /// <summary>
        /// If true, the button text will display the region and color id.
        /// </summary>
        public bool VerboseButtonTexts { get; set; }

        public RegionColorChooser()
        {
            InitializeComponent();

            _buttonColors = new Button[Species.ColorRegionCount];
            _panels = new Panel[Species.ColorRegionCount];
            for (int i = 0; i < Species.ColorRegionCount; i++)
            {
                var p = new Panel { Width = 27, Height = 27, Margin = new Padding(1) };
                _panels[i] = p;
                var b = new Button { Width = 23, Height = 23, Left = 2, Top = 2, Text = i.ToString() };
                var ii = i;
                b.Click += (s, e) => ChooseColor(ii, b);
                _buttonColors[i] = b;
                p.Controls.Add(b);
                flowLayoutPanel1.Controls.Add(p);
            }

            _selectedRegionColorIds = new int[Species.ColorRegionCount];
            ColorRegionsUseds = new bool[Species.ColorRegionCount];
            _colorPicker = new MyColorPicker();
            _tt.AutoPopDelay = 7000;
            Disposed += RegionColorChooser_Disposed;
        }

        public void SetOneButtonPerRow(bool onePerRow)
        {
            foreach (var b in _buttonColors)
                flowLayoutPanel1.SetFlowBreak(b, onePerRow);
        }

        public void SetSpecies(Species species, int[] colorIDs)
        {
            _selectedRegionColorIds = colorIDs.ToArray();

            if (species?.colors != null)
                _colorRegions = species.colors;
            else
            {
                // species-info is not available, show all region-buttons
                _colorRegions = new List<ColorRegion>();
                for (int i = 0; i < Species.ColorRegionCount; i++)
                {
                    _colorRegions.Add(new ColorRegion());
                }
            }
            for (int r = 0; r < _buttonColors.Length; r++)
            {
                ColorRegionsUseds[r] = !string.IsNullOrEmpty(_colorRegions[r]?.name);
                _panels[r].Visible = ColorRegionsUseds[r];

                if (ColorRegionsUseds[r])
                {
                    SetColorButton(_buttonColors[r], r);
                }
            }
        }

        public int[] ColorIDs => _selectedRegionColorIds.ToArray();

        public void Clear()
        {
            SetColorIds(new int[_buttonColors.Length]);
        }

        /// <summary>
        /// Set colors to random values in the range (0 .. 99).
        /// </summary>
        internal void RandomColors()
        {
            var colorIds = new int[_buttonColors.Length];
            var rand = new Random();
            for (int r = 0; r < colorIds.Length; r++)
                colorIds[r] = rand.Next(99) + 1;

            SetColorIds(colorIds);
        }

        /// <summary>
        /// Set colors to random values in the set of natural occurring colors of the species.
        /// </summary>
        internal void RandomNaturalColors(Species species)
        {
            SetColorIds(species?.RandomSpeciesColors());
        }

        public void SetColorIds(int[] colorIds)
        {
            if (colorIds == null)
            {
                Clear();
                return;
            }

            var l = Math.Min(_buttonColors.Length, colorIds.Length);
            for (int r = 0; r < l; r++)
            {
                _selectedRegionColorIds[r] = colorIds[r];
                SetColorButton(_buttonColors[r], r);
            }
            RegionColorChosen?.Invoke();
        }

        private void ChooseColor(int region, Button sender)
        {
            if (!_colorPicker.isShown && _colorRegions != null && region < Species.ColorRegionCount)
            {
                _colorPicker.SetColors(_selectedRegionColorIds[region], _colorRegions[region].name, _colorRegions[region]?.naturalColors);
                if (_colorPicker.ShowDialog() == DialogResult.OK)
                {
                    // color was chosen
                    _selectedRegionColorIds[region] = _colorPicker.SelectedColorId;
                    SetColorButton(sender, region);
                    RegionColorChosen?.Invoke();
                }
            }
        }

        private void SetColorButton(Button bt, int region)
        {
            int colorId = _selectedRegionColorIds[region];
            bt.SetBackColorAndAccordingForeColor(CreatureColors.CreatureColor(colorId));
            if (VerboseButtonTexts)
                bt.Text = $"[{region}]: {colorId}";
            // tooltip
            if (_colorRegions?[region] != null)
                _tt.SetToolTip(bt, $"{_colorRegions[region].name} ({region}):\n{CreatureColors.CreatureColorName(colorId)} ({colorId})");
        }

        private void RegionColorChooser_Disposed(object sender, EventArgs e)
        {
            _tt.RemoveAll();
        }

        /// <summary>
        /// True if a color is new in this species.
        /// </summary>
        internal bool ColorNewInSpecies;
        /// <summary>
        /// True if color is new in this region (but exists in other region in this species).
        /// </summary>
        internal bool ColorNewInRegion;

        internal void SetRegionColorsExisting(CreatureCollection.ColorExisting[] colorAlreadyAvailable)
        {
            ColorNewInRegion = false;
            ColorNewInSpecies = false;

            var parameter = CreatureCollection.ColorExisting.Unknown;
            for (int ci = 0; ci < Species.ColorRegionCount; ci++)
            {
                if (colorAlreadyAvailable != null)
                    parameter = colorAlreadyAvailable[ci];
                switch (parameter)
                {
                    case CreatureCollection.ColorExisting.ColorIsNew:
                        _panels[ci].BackColor = Color.Gold;
                        ColorNewInSpecies = true;
                        break;
                    case CreatureCollection.ColorExisting.ColorExistingInOtherRegion:
                        _panels[ci].BackColor = Color.DarkGreen;
                        ColorNewInRegion = true;
                        break;
                    default:
                        _panels[ci].BackColor = Color.Transparent; break;
                }
            }
        }
    }
}
