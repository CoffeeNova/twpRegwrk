using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Management;
using System.Reflection;


namespace NWTweak
{
    public class RegistryWorker
    {
        /// <summary>
        /// Удаление значения ключа из реестра.
        /// </summary>
        /// <param name="rootKey">Раздел HKEY.</param>
        /// <param name="keyLocation">Путь ключа.</param>
        /// <param name="valueName">Имя значения.</param>
        /// <returns><see langword="true"/> - если удаление прошло успешно</returns>
        public static bool DeleteKey(RegistryHive rootKey, String keyLocation, String valueName)
        {
            bool res = false;
            using (var rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                                     Environment.Is64BitOperatingSystem
                                         ? RegistryView.Registry64
                                         : RegistryView.Registry32))
            {
                using(var sKey = rKey.OpenSubKey(keyLocation, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (sKey != null)
                    {
                        sKey.DeleteValue(valueName);
                        res = true;
                    }
                    else
                        throw new System.IO.IOException("Не найден ключ " + keyLocation); //ошибка "дириктория подключа задана неверно"
                    sKey.Close();
                }
            }
            return res;
        }

        /// <summary>
        /// Ищет первое же попавшееся вхождние значения <paramref name="valueName"/>  в ключе <paramref name="subKeyLocation"/>
        /// </summary>
        /// <param name="rootKey">Раздел HKEY</param>
        /// <param name="subKeyLocation">Путь ключа в котором производить поиск </param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        public static string FindSubKey(RegistryHive rootKey, String subKeyLocation, String valueName)
        {
            string[] SubKeyNames;
            string SubKey = "";
            string[] KeyNamesList;
            RegistryKey Rkey = RegistryKey.OpenBaseKey(rootKey,
                                        Environment.Is64BitOperatingSystem
                                            ? RegistryView.Registry64
                                            : RegistryView.Registry32);

            Rkey = Rkey.OpenSubKey(subKeyLocation);
            if (Rkey != null)
            {
                SubKeyNames = (string[])Rkey.GetSubKeyNames();
                for (int SubKeyNumb = 0; SubKeyNumb < SubKeyNames.Length; SubKeyNumb++)
                {
                    SubKey = SubKeyNames[SubKeyNumb];
                    RegistryKey O_SubKey = Registry.LocalMachine.OpenSubKey(subKeyLocation + "\\" + SubKey);
                    KeyNamesList = (string[])O_SubKey.GetValueNames();
                    for (int kName = 0; kName < KeyNamesList.Length; kName++)
                    {
                        if (KeyNamesList[kName] == valueName)
                        {
                            O_SubKey.Close();
                            return SubKey;
                        }
                        else
                        {//ошибка "ни в одном подключе не найдет ключ с именем %keyName"
                        }
                    }
                    O_SubKey.Close();
                }
            }
            else
            {//ошибка "дириктория подключа задана неверно"
            }
            if (Rkey != null)
                Rkey.Close();
            return SubKey;
        }
        public List<string> GetSubKeyNames(string path)
        {
            List<string> res = new List<string>();
            string root;
            RegistryKey rSubkey = Registry.ClassesRoot;
            string[] subKeyNames = rSubkey.GetSubKeyNames();

            foreach (string s in subKeyNames)
            {
                res.Add(s);
            }
            rSubkey.Close();
            return res;
        }

        /// <summary>
        /// Получает все значения типа <typeparamref name="T"/> в ключе <paramref name="keyLocation"/> 
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="rootKey">Раздел HKEY</param>
        /// <param name="keyLocation">Путь ключа</param>
        /// <returns>Список, <see langword="null"/> - при неудачной операции, или пустом ключе</returns>
        /// <remarks>Для получения значений всех типов, следует указать тип <typeparamref name="T"/> как <see langword="object"/></remarks>
        /// <exception cref="System.IO.IOException">Произошла системная ошибка, например, был удален текущий раздел</exception>
        /// <exception cref="System.ArgumentException">Недопустимое значение параметра rootKey</exception>
        /// <exception cref="System.UnauthorizedAccessException">У пользователя отсутствуют необходимые права доступа к реестру</exception>
        /// <exception cref="System.ArgumentNullException">name имеет значение <see langword="null"/></exception>
        /// <exception cref="System.ObjectDisposedException">Раздел Microsoft.Win32.RegistryKey является закрытым (доступ к закрытым разделам невозможен)</exception>
        /// <exception cref="System.Security.SecurityException">У пользователя отсутствуют разрешения, необходимые для доступа к разделу реестра в заданном режиме</exception>
        public static List<KeyValuePair<string, T>> GetValuesFromKey<T>(RegistryHive rootKey, string keyLocation)
        {
            var valuesList = new List<KeyValuePair<string, T>>();

            using (var rKey = RegistryKey.OpenBaseKey(rootKey,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
            {
                using (var sKey = rKey.OpenSubKey(keyLocation))
                {
                    if (sKey != null)
                    {
                        string[] valueNames = sKey.GetValueNames();
                        if (valueNames.Count() > 0)
                            foreach (string valueName in valueNames)
                            {
                                try
                                {
                                    valuesList.Add(new KeyValuePair<string, T>(valueName, (T)sKey.GetValue(valueName)));
                                }
                                catch { }
                            }
                    }
                    else
                        throw new System.IO.IOException("Не найден ключ " + keyLocation); //ошибка "дириктория подключа задана неверно"
                    sKey.Close();
                }
                rKey.Close();
            }
            return valuesList;
        }

        /// <summary>
        /// Возвращает все имена сабключей в в ключе <paramref name="keyLocation"/>
        /// </summary>
        /// <param name="rootKey">Раздел HKEY</param>
        /// <param name="keyLocation">Путь ключа</param>
        /// <exception cref="System.IO.IOException">Произошла системная ошибка, например, был удален текущий раздел</exception>
        /// <exception cref="System.ArgumentException">Недопустимое значение параметра rootKey</exception>
        /// <exception cref="System.UnauthorizedAccessException">У пользователя отсутствуют необходимые права доступа к реестру</exception>
        /// <exception cref="System.ArgumentNullException">name имеет значение <see langword="null"/></exception>
        /// <exception cref="System.ObjectDisposedException">Раздел Microsoft.Win32.RegistryKey является закрытым (доступ к закрытым разделам невозможен)</exception>
        /// <exception cref="System.Security.SecurityException">У пользователя отсутствуют разрешения, необходимые для доступа к разделу реестра в заданном режиме</exception>
        /// <returns>Список имен сабключей</returns>
        public static List<string> GetSubKeyNames(RegistryHive rootKey, string keyLocation)
        {
            var subKeysList = new List<string>();

            using (var rKey = RegistryKey.OpenBaseKey(rootKey,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
            {
                using (var sKey = rKey.OpenSubKey(keyLocation))
                {
                    if (sKey != null)
                        subKeysList = sKey.GetSubKeyNames().ToList();
                    else
                        throw new System.IO.IOException("Не найден ключ " + keyLocation); //ошибка "дириктория подключа задана неверно"
                    sKey.Close();
                }
                rKey.Close();
            }
            return subKeysList;
        }

        /// <summary>
        /// Чтение значения ключа из реестра типа <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="rootKey">Раздел HKEY</param>
        /// <param name="keyLocation">Путь ключа</param>
        /// <param name="valueName">Имя значения</param>
        /// <returns>Значение ключа, <see langword="null"/>, если пара-значение отсутствует в реестре </returns>
        /// <remarks> Для получения значений всех типов, следует указать тип <typeparamref name="T"/> как <see langword="object"/></remarks>
        /// <exception cref="System.IO.IOException">Произошла системная ошибка, например, был удален текущий раздел</exception>
        /// <exception cref="System.ArgumentException">Недопустимое значение параметра rootKey</exception>
        /// <exception cref="System.UnauthorizedAccessException">У пользователя отсутствуют необходимые права доступа к реестру</exception>
        /// <exception cref="System.ArgumentNullException">name имеет значение <see langword="null"/></exception>
        /// <exception cref="System.ObjectDisposedException">Раздел Microsoft.Win32.RegistryKey является закрытым (доступ к закрытым разделам невозможен)</exception>
        /// <exception cref="System.Security.SecurityException">У пользователя отсутствуют разрешения, необходимые для доступа к разделу реестра в заданном режиме</exception>
        public static T GetKeyValue<T>(RegistryHive rootKey, string keyLocation, string valueName)
        {
            T value;
            using (var rKey = RegistryKey.OpenBaseKey(rootKey,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
            {
                using (var sKey = rKey.OpenSubKey(keyLocation))
                {
                    if (sKey != null)
                        value = (T)sKey.GetValue(valueName);
                    else
                        throw new System.IO.IOException("Не найден ключ " + keyLocation); //ошибка "дириктория подключа задана неверно"
                    sKey.Close();
                }
            }
            return value;
        }


        /// <summary>
        /// Записывает значение ключа в реестр
        /// </summary>
        /// <param name="rootKey"></param>
        /// <param name="keyLocation"></param>
        /// <param name="valueKind"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteKeyValue(RegistryHive rootKey, String keyLocation, RegistryValueKind valueKind, String valueName, object value)
        {
            bool res = false;

            using (RegistryKey rKey = RegistryKey.OpenBaseKey(rootKey, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
            {
                using (var cKey = rKey.CreateSubKey(keyLocation, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (cKey != null)
                    {
                        cKey.SetValue(valueName, value, valueKind);
                        cKey.Close();
                        res = true;
                    }
                    else
                        throw new System.IO.IOException("Не найден ключ " + keyLocation); //ошибка "дириктория подключа задана неверно"
                    cKey.Close();
                }
                rKey.Close();
            }
            return res;
        }

        /// <summary>
        /// Создает новый вложенный раздел или открывает существующий вложенный раздел с доступом на запись.
        /// </summary>
        /// <param name="rootKey">Раздел HKEY.</param>
        /// <param name="subKey">Имя или путь вложенного раздела, создаваемого или открываемого.В этой строке не учитывается регистр знаков.</param>
        /// <returns><see langword="true"/></exception>, если удалось создать раздел.</returns>
        /// <exception cref="System.IO.IOException">Уровень вложенности превосходит 510.— или —Произошла системная ошибка, например удаление раздела или попытка создать раздел в корне Microsoft.Win32.Registry.LocalMachine.</exception>
        /// <exception cref="System.ArgumentException">Недопустимое значение параметра rootKey</exception>
        /// <exception cref="System.UnauthorizedAccessException">У пользователя отсутствуют необходимые права доступа к реестру</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="subkey"/> имеет значение <see langword="null"/></exception>
        /// <exception cref="System.ObjectDisposedException">Раздел Microsoft.Win32.RegistryKey является закрытым (доступ к закрытым разделам невозможен)</exception>
        /// <exception cref="System.Security.SecurityException">У пользователя отсутствуют разрешения, необходимые для создания раздела реестра.</exception>
        public static bool CreateSubKey(RegistryHive rootKey, String subKey)
        {
            bool result = false;
            using (var rKey = RegistryKey.OpenBaseKey(rootKey,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
            {
                rKey.CreateSubKey(subKey);
                result = true;
                rKey.Close();
            }
            return result;
        }
    }

    public class RegistryHelpers
    {
        public static RegistryKey GetRegistryKey(string keyPath)
        {
            RegistryKey localMachineRegistry
                = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                                          Environment.Is64BitOperatingSystem
                                              ? RegistryView.Registry64
                                              : RegistryView.Registry32);

            return string.IsNullOrEmpty(keyPath)
                ? localMachineRegistry
                : localMachineRegistry.OpenSubKey(keyPath);
        }

        public static object GetRegistryValue(string keyPath, string keyName)
        {
            RegistryKey registry = GetRegistryKey(keyPath);
            return registry.GetValue(keyName);
        }
    }

}
