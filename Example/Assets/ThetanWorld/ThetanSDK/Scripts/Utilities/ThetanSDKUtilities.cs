using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;
using Random = UnityEngine.Random;

namespace ThetanSDK.Utilities
{
    public static class ThetanSDKUtilities
    {
        public enum CustomSize
        {
            Default = 0,
            Size_64 = 1,
            Size_128 = 2,
            Size_256 = 3,
        }
        
        public static string GetUrlPathHeroFullAvatar(GameWorldType gameType, int skinId, CustomSize customSize = CustomSize.Default)
        {
            var customPosfixSize = GetCustomImgPostfix(customSize);
            if (string.IsNullOrEmpty(customPosfixSize))
            {
                return $"/resources/{GetResourceGameFolderName(gameType)}/hero/full_avatar/{skinId}.png";
            }
            else
            {
                return string.Format("/resources/{0}/hero/full_avatar/{1}{2}.png", 
                    GetResourceGameFolderName(gameType), skinId, customPosfixSize);
            }
        }

        private static string GetCustomImgPostfix(CustomSize customSize)
        {
            if (customSize == CustomSize.Default)
                return string.Empty;

            switch (customSize)
            {
                case CustomSize.Size_64:
                    return "_64";
                case CustomSize.Size_128:
                    return "_128";
                case CustomSize.Size_256:
                    return "_256";
            }
            
            return string.Empty;
        }
        
        public static string GetUrlPathHeroAvatar(GameWorldType gameType, int skinId)
        {
            return $"/resources/{GetResourceGameFolderName(gameType)}/hero/sdk_avatar/{skinId}.png";
        }

        public static string GetUrlPathGameIcon(GameWorldType gameType)
        {
            return $"/GameIcon/{(int)gameType}.png";
        }

        public static string GetUrlImageProfileCosmetic(int cosmeticId)
        {
            return $"/cosmetics/cosmetic_{cosmeticId}.png";
        }
        
        private static string GetResourceGameFolderName(GameWorldType gameType)
        {
            switch (gameType)
            {
                case GameWorldType.ThetanArena:
                    return "thetan_arena";
                case GameWorldType.ThetanRivals:
                    return "thetan_rivals";
                case GameWorldType.ThetanImmortal:
                    return "thetan_immortal";
                default:
                    return "unknown";
            }
        }
        
        public static Sprite CreateSpriteFromTexture2D(Texture2D texture2D)
        {
            if (texture2D == null)
                return null;
            
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f),
                100f, 0U, SpriteMeshType.FullRect);
        }

        public static string GetHeroRarityName(NFTRarity rarity)
        {
            switch (rarity)
            {
                case NFTRarity.Common:
                    return ThetanSDKConstantString.HERO_RARITY_COMMON;
                case NFTRarity.Rare:
                    return ThetanSDKConstantString.HERO_RARITY_RARE;
                case NFTRarity.Epic:
                    return ThetanSDKConstantString.HERO_RARITY_EPIC;
                case NFTRarity.Legend:
                    return ThetanSDKConstantString.HERO_RARITY_LEGEND;
                default:
                    return ThetanSDKConstantString.HERO_RARITY_UNKNOWN;
            }
        }
        
        public static string GetWorldName(GameWorldType gameWorldType)
        {
            switch (gameWorldType)
            {
                case GameWorldType.ThetanArena:
                    return ThetanSDKConstantString.GAME_WORLD_NAME_THETAN_ARENA;
                case GameWorldType.ThetanRivals:
                    return ThetanSDKConstantString.GAME_WORLD_NAME_THETAN_RIVALS;
                case GameWorldType.ThetanUGC:
                    return ThetanSDKConstantString.GAME_WORLD_NAME_THETAN_UGC;
                case GameWorldType.ThetanImmortal:
                    return ThetanSDKConstantString.GAME_WORLD_NAME_THETAN_IMMORTAL;
                case GameWorldType.ThetanMarket:
                    return ThetanSDKConstantString.GAME_WORLD_NAME_THETAN_MARKET;
                default:
                    return ThetanSDKConstantString.GAME_WORLD_NAME_UNKNOWN;
            }
        }

        public static string ToStringTimeShort(this float totalSecond)
        {
            return ToStringTimeShort(TimeSpan.FromSeconds(totalSecond));
        }

        public static string ToStringTime(this float totalSecond)
        {
            return ToStringTime(TimeSpan.FromSeconds(totalSecond));
        }

        public static string ToStringTime(TimeSpan timeSpan)
        {
            string strTimer = string.Empty;
            try
            {
                if (timeSpan.Days > 30)
                {
                    strTimer = $"{timeSpan.Days / 30}M {timeSpan.Days % 30}d";
                }
                else if (timeSpan.Days > 0)
                {
                    strTimer = $"{timeSpan.Days}d {timeSpan.Hours}h";
                }
                else if (timeSpan.Hours > 0)
                {
                    strTimer = $"{timeSpan.Hours}h {timeSpan.Minutes}m";
                }
                else if(timeSpan.Minutes > 0)
                {
                    strTimer = $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
                }
                else
                {
                    strTimer = $"{timeSpan.Seconds}s";
                }
            }
            catch (Exception ex)
            {
            }

            return strTimer;
        }

        public static string ToStringTimeShort(TimeSpan timeSpan)
        {
            string strTimer = string.Empty;
            try
            {
                if (timeSpan.Days > 30)
                {
                    strTimer = $"{timeSpan.Days / 30}M";
                }
                else if (timeSpan.Days > 0)
                {
                    strTimer = $"{timeSpan.Days}d";
                }
                else if (timeSpan.Hours > 0)
                {
                    strTimer = $"{timeSpan.Hours}h";
                }
                else if(timeSpan.Minutes > 0)
                {
                    strTimer = $"{timeSpan.Minutes}m";
                }
                else
                {
                    strTimer = $"{timeSpan.Seconds}s";
                }
            }
            catch (Exception ex)
            {
            }

            return strTimer;
        }

        public static string ToStringTimeDigitalDigit(TimeSpan timeSpan)
        {
            string strTimer = string.Empty;
            try
            {
                if (timeSpan.Days > 30)
                {
                    strTimer = string.Format("{0:00}:{1:00}", timeSpan.Days / 30, timeSpan.Days % 30);
                }
                else if (timeSpan.Days > 0)
                {
                    strTimer = string.Format("{0:00}:{1:00}", timeSpan.Days, timeSpan.Hours);
                }
                else if (timeSpan.Hours > 0)
                {
                    strTimer = string.Format("{0:00}:{1:00}", timeSpan.Hours, timeSpan.Minutes);
                }
                else if(timeSpan.Minutes > 0)
                {
                    strTimer = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
                }
                else
                {
                    strTimer = string.Format("00:{0:00}", timeSpan.Seconds);
                }
            }
            catch (Exception ex)
            {
            }

            return strTimer;
        }
         
         const string FloatingPointSpecificDigitStr_0 = "{0:0}";
         const string FloatingPointSpecificDigitStr_1 = "{0:0.0}";
         const string FloatingPointSpecificDigitStr_2 = "{0:0.00}";
         const string FloatingPointSpecificDigitStr_3 = "{0:0.000}";
         const string FloatingPointSpecificDigitStr_4 = "{0:0.0000}";
         /// <summary>
         /// format kiểu float
         /// </summary>
         /// <param name="floatingPoint"></param>
         /// <param name="decimals"></param>
         /// <returns></returns>
         public static string FormatUnitFloatWithSpecificDigits(this float floatingPoint, int numberFormat = 2)
         {
             numberFormat = Mathf.Clamp(numberFormat, 0, 4);
             switch (numberFormat)
             {
                 case 0:
                     return string.Format(FloatingPointSpecificDigitStr_0, floatingPoint);
                 case 1:
                     return string.Format(FloatingPointSpecificDigitStr_1, floatingPoint);
                 case 2:
                     return string.Format(FloatingPointSpecificDigitStr_2, floatingPoint);
                 case 3:
                     return string.Format(FloatingPointSpecificDigitStr_3, floatingPoint);
                 default:
                     return string.Format(FloatingPointSpecificDigitStr_4, floatingPoint);
             }
         }

         /// <summary>
         /// Input range [0, 100]
         /// </summary>
         internal static string FormatUnitPercent(this float floatingPoint)
         {
             var roundedNumber = Math.Round(floatingPoint, 1);
             return FormatUnitFloatWithSpecificDigits((float)roundedNumber, 1);
         }

         internal static string FormatUnitTime(this float floatingPoint)
         {
             return FormatUnitFloatWithSpecificDigits(Mathf.RoundToInt(floatingPoint), 1);
         }

         public static string FormatUnitCurrency(this float floatingPoint)
         {
             var absFloatingPoint = Mathf.Abs(floatingPoint);
             float intergerPart = (int)absFloatingPoint;
             var decimalPart = absFloatingPoint % 1;

             intergerPart = Mathf.Max(intergerPart, 0.1f);

             var k = decimalPart * 100 / intergerPart;

             var roundNumber = 0;
             if (k <= 0.5f)
             {
                 roundNumber = 0;
             }
             else if (k <= 2.5)
             {
                 roundNumber = 1;
             }
             else if (k <= 5)
             {
                 roundNumber = 2;
             }
             else if (k <= 10000)
             {
                 roundNumber = 3;
             }
             else
             {
                 roundNumber = 3;
             }

             var roundedInput = Math.Round(floatingPoint, roundNumber);
             return ((float)roundedInput).FormatUnitFloatWithSpecificDigits(roundNumber);
         }

         public static int ConvertSecondToMinute(this float second)
         {
             return (int)(second / 60);
         }

         public static void SetAlphaImg(this Image img, float alphaValue)
         {
             var color = img.color;
             color.a = alphaValue;
             img.color = color;
         }

         static DateTime TIME_MILESTONE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
         public static long GetLocalTimestamp()
         {
             System.Int32 unixTimestamp = (System.Int32)(System.DateTime.UtcNow.Subtract(TIME_MILESTONE)).TotalSeconds;
             
             return unixTimestamp;
         }
         
         internal static string Hash(string input, string key)
         {
             // UnityEngine.Debug.Log("Hash input " + input + key);

             // Convert the input string to a byte array and compute the hash.
             var hash = System.Security.Cryptography.SHA256.Create();

             byte[] data = hash.ComputeHash(Encoding.UTF8.GetBytes(input + key));

             // Create a new Stringbuilder to collect the bytes
             // and create a string.
             StringBuilder sBuilder = new StringBuilder();

             // Loop through each byte of the hashed data 
             // and format each one as a hexadecimal string.
             for (int i = 0; i < data.Length; i++)
             {
                 sBuilder.Append(data[i].ToString("x2"));
             }

             // Return the hexadecimal string.
             return sBuilder.ToString();
         }

         private static TextEditor _textEditor;
         public static void CopyToClipboard(this string txt)
         {
             if(_textEditor == null)
             {
                 _textEditor = new TextEditor()
                 {
                     text = txt,
                 };
             }
             else
             {
                 _textEditor.text = txt;
             }
             _textEditor.SelectAll();
             _textEditor.Copy();
         }

         public static string ConvertMiddleStringToElipsis(string str, int numberCharacterHeadReserve,
             int numberCharacterTailReserve)
         {
             if (string.IsNullOrEmpty(str))
                 return str;
             
             if (numberCharacterHeadReserve + numberCharacterTailReserve >= str.Length)
                 return str;

             string result = string.Empty;
             
             var strBuilder = Utils.StringBuilderPool.Get();
             strBuilder.Clear();
             for (int i = 0; i < numberCharacterHeadReserve; i++)
             {
                 strBuilder.Append(str[i]);
             }
                 
             strBuilder.Append("...");

             for (int i = str.Length - numberCharacterTailReserve; i < str.Length; i++)
             {
                 strBuilder.Append(str[i]);
             }

             result = strBuilder.ToString();
             Utils.StringBuilderPool.Release(strBuilder);
             return result;
         }

         public static void Shuffle<T>(List<T> list, int maximumNumberShuffle = -1)
         {
             if (list == null || list.Count == 0 ||
                 list.Count == 1)
                 return;

             if (maximumNumberShuffle == -1)
                 maximumNumberShuffle = list.Count;

             int numberOfShuffle = maximumNumberShuffle;

             while (numberOfShuffle > 1)
             {
                 numberOfShuffle--;

                 int item1Index = UnityEngine.Random.Range(0, list.Count);
                 int item2Index = UnityEngine.Random.Range(0, list.Count);

                 (list[item1Index], list[item2Index]) = (list[item2Index], list[item1Index]);
             }
         }

         /// <param name="minValue">Inclusive</param>
         /// <param name="maxValue">Inclusive</param>
         /// <returns></returns>
         public static Vector2 RandomVector2(float minValue, float maxValue)
         {
             Vector2 baseVec = new Vector3(
                 UnityEngine.Random.Range(-1f, 1f),
                 UnityEngine.Random.Range(-1f, 1f)).normalized;
             return baseVec * UnityEngine.Random.Range(minValue, maxValue);
         }

         public static bool RandomBool()
         {
             return UnityEngine.Random.Range(0, 101) >= 50;
         }
    }
}

