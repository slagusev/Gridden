﻿using System;
using System.IO;

namespace Gridden
{
    public static class MapFactory
    {
        private const string DefaultMapTitle = "New map";
        private const int DefaultMapWidth = 12;
        private const int DefaultMapHeight = 8;

        public static Map BuildNew()
        {
            return new Map(DefaultMapTitle, DefaultMapWidth, DefaultMapHeight);
        }

        public static Map BuildNew(string name, int w, int h)
        {
            return new Map(name, w, h);
        }

        public static Map FromFile(string fileName)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(fileName);
                string[] lines = File.ReadAllLines(fileName);
                
                // validation
                if (ValidateMap(name, lines))
                {
                    Map map = BuildNew(name, lines[0].Length, lines.Length);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        for (int j = 0; j < lines[0].Length; j++)
                        {
                            map.SetCharAtPosition(j, i, lines[i][j]);
                        }
                    }

                    return map;
                }
                else
                {
                    throw new Exception("Invalid map file!");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static bool ValidateMap(string name, string[] lines)
        {
            return true;
        }
    }
}