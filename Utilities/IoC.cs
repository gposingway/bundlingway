using Serilog;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;

namespace Bundlingway.Utilities
{
    public static class IoC
    {
        private static readonly object Lock;

        public static readonly ConcurrentDictionary<string, Assembly> AssemblyLoadMap;

        private static readonly ConcurrentDictionary<string, string> AssemblyPathMap;

        private static readonly ConcurrentDictionary<Type, List<Type>> TypeResolutionMap;

        public static readonly Dictionary<Type, List<Type>> GetGenericsByBaseClassCache;

        private static readonly object GetGenericsByBaseClassLock;

        private static readonly List<string> IgnoreList;

        static IoC()
        {
            Lock = new object();
            AssemblyLoadMap = [];
            AssemblyPathMap = [];
            TypeResolutionMap = [];
            GetGenericsByBaseClassCache = [];
            GetGenericsByBaseClassLock = new object();
            IgnoreList =
        [
            "System.", "Microsoft.", "mscorlib", "netstandard", "Serilog.", "ByteSize", "AWSSDK.", "StackExchange.", "SixLabors.", "BouncyCastle.",
            "MongoDB.", "Dapper", "SharpCompress", "Remotion", "Markdig", "Westwind", "Serilog", "DnsClient", "Oracle"
        ];
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                RegisterAssembly(entryAssembly);
            }

            int num = -1;
            int num2 = 0;
            while (num2 != num && num2 < 10) // Add safety limit to prevent infinite loops
            {
                num = num2;
                foreach (KeyValuePair<string, Assembly> item in AssemblyLoadMap.ToList()) // ToList to avoid collection modification
                {
                    try
                    {
                        item.Value.GetTypes();
                        num2++;
                    }
                    catch (Exception e)
                    {
                        Log.Information("Error while loading " + item.Key);
                    }
                }
            }

            Log.Information($"Assembly Loader - {AssemblyLoadMap.Count} assemblies registered");
        }

        public static List<T> GetInstances<T>(bool excludeCoreNullDefinitions = true) where T : class
        {
            return (from i in GetClassesByInterface<T>(excludeCoreNullDefinitions)
                    select i.CreateInstance<T>()).ToList();
        }

        private static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault((assembly) => assembly.GetName().Name == name);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return GetAssemblyByName(args.Name.Split(',')[0]);
        }

        private static void LoadAssembliesFromDirectory(string path)
        {
            if (path == null)
            {
                return;
            }

            if (path.IndexOf(";", StringComparison.Ordinal) > -1)
            {
                foreach (string item in path.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    LoadAssembliesFromDirectory(item);
                }

                return;
            }

            if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string[] files = Directory.GetFiles(path, "*.dll");
                for (int i = 0; i < files.Length; i++)
                {
                    LoadAssembly(files[i]);
                }

                return;
            }

            lock (Lock)
            {
                LoadAssembly(path);
            }
        }

        public static Assembly RegisterAssembly(Assembly assy)
        {
            string assyName = assy.GetName().Name;
            if (IgnoreList.Any((j) => assyName.StartsWith(j)))
            {
                return null;
            }

            if (AssemblyLoadMap.ContainsKey(assyName))
            {
                return assy;
            }

            AssemblyLoadMap[assyName] = assy;
            List<AssemblyName> list = (from i in assy.GetReferencedAssemblies()
                                       where !IgnoreList.Any((j) => i.Name.StartsWith(j))
                                       select i).ToList();
            if (!list.Any())
            {
                return assy;
            }

            foreach (AssemblyName item in list)
            {
                RegisterAssembly(AssemblyLoadContext.Default.LoadFromAssemblyName(item));
            }

            return assy;
        }

        private static void LoadAssembly(string physicalPath)
        {
            if (physicalPath == null)
            {
                return;
            }

            try
            {
                string fileName = Path.GetFileName(physicalPath);
                if (!AssemblyPathMap.ContainsKey(fileName))
                {
                    AssemblyPathMap.TryAdd(fileName, physicalPath);
                    RegisterAssembly(Assembly.LoadFrom(physicalPath));
                }
            }
            catch (Exception ex)
            {
                if (ex is ReflectionTypeLoadException ex2)
                {
                    ex2.LoaderExceptions.ToList();
                }
            }
        }

        public static List<Type> TypeByName(string typeName)
        {
            return AssemblyLoadMap.Values.ToList().SelectMany((i) => from j in i.GetTypes()
                                                                     where !j.IsInterface && !j.IsAbstract && (j.Name.Equals(typeName) || (j.FullName?.Equals(typeName) ?? false))
                                                                     select j).ToList();
        }

        public static List<Type> GetClassesByBaseClass(Type refType, bool limitToMainAssembly = false)
        {
            try
            {
                List<Type> list = new List<Type>();
                List<Assembly> list2 =
                [
                    Assembly.GetExecutingAssembly(),
                ];

                foreach (Assembly item in list2)
                {
                    list.AddRange(from type in item.GetTypes()
                                  where type.BaseType != null
                                  where type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == refType || type.BaseType == refType
                                  select type);
                }

                return list;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Exception[] loaderExceptions = ex.LoaderExceptions;
                for (int i = 0; i < loaderExceptions.Length; i++)
                {
                    _ = loaderExceptions[i];
                }

                throw ex;
            }
            catch (Exception ex2)
            {
                throw ex2;
            }
        }

        public static List<Type> GetGenericsByBaseClass(Type refType)
        {
            lock (GetGenericsByBaseClassLock)
            {
                if (GetGenericsByBaseClassCache.ContainsKey(refType))
                {
                    return GetGenericsByBaseClassCache[refType];
                }

                List<Type> list = new List<Type>();
                try
                {
                    foreach (Assembly item in AssemblyLoadMap.Values.ToList())
                    {
                        Type[] types = item.GetTypes();
                        foreach (Type type in types)
                        {
                            if (type.BaseType == null || !type.BaseType.IsGenericType || type == refType)
                            {
                                continue;
                            }

                            try
                            {
                                Type[] genericTypeArguments = type.BaseType.GenericTypeArguments;
                                for (int j = 0; j < genericTypeArguments.Length; j++)
                                {
                                    if (genericTypeArguments[j] == refType)
                                    {
                                        list.Add(type);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }

                    GetGenericsByBaseClassCache.Add(refType, list);
                }
                catch (Exception)
                {
                }

                return list;
            }
        }

        public static IEnumerable<Type> GetClassesByInterface<T>(bool excludeFallback = false)
        {
            return GetClassesByInterface(typeof(T), excludeFallback);
        }

        public static IEnumerable<Type> GetClassesByInterface(Type targetType, bool excludeFallback = true)
        {
            lock (Lock)
            {
                if (!TypeResolutionMap.ContainsKey(targetType))
                {
                    Assembly.GetExecutingAssembly();
                    List<Type> list = new List<Type>();
                    foreach (Assembly value2 in AssemblyLoadMap.Values)
                    {
                        try
                        {
                            IEnumerable<Type> collection = from target in value2.GetTypes()
                                                           where !target.IsInterface
                                                           where !target.IsAbstract
                                                           where targetType.IsInterface ? target.GetInterfaces().Contains(targetType) : targetType.IsAssignableFrom(target)
                                                           where targetType != target
                                                           select target;
                            list.AddRange(collection);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ReflectionTypeLoadException)
                            {
                                (ex as ReflectionTypeLoadException).LoaderExceptions.ToList();
                            }
                        }
                    }


                    TypeResolutionMap.TryAdd(targetType, list);
                }

                return TypeResolutionMap[targetType];
            }
        }

        public static IEnumerable<T> CreateInstances<T>(this IEnumerable<Type> typeRefs)
        {
            return typeRefs.Select(CreateInstance<T>);
        }

        public static T CreateInstance<T>(this Type typeRef)
        {
            try
            {
                return (T)Activator.CreateInstance(typeRef);
            }
            catch (Exception innerException)
            {
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }

                throw innerException;
            }
        }
    }
}
