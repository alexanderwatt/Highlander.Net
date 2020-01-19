using System;
using Microsoft.Practices.Unity.Configuration;

namespace Orion.ModelFramework.Helpers
{
    ///<summary>
    ///</summary>
    public static class DependencyCreator
    {
        /// <summary>
        /// Resolves the specified container name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="containerName">Name of the container.</param>
        /// <returns></returns>
        public static T Resolve<T>(string containerName)
        {
            T results=default(T);
            UnityContainerElement container = UnityHelper.GetContainer(containerName);
            Type theTypeImInterestedIn = typeof(T);
            UnityTypeElement matchedType = null;
            Boolean bMatchFound = false;
            foreach (UnityTypeElement typeElement in container.Types)
            {
                string[] typeParts = typeElement.TypeName.Split(',');
                string typeNameDef = $"{typeParts[0]},{typeParts[1]}";
                string typeMapDef =
                    $"{theTypeImInterestedIn.Namespace}.{theTypeImInterestedIn.Name},{theTypeImInterestedIn.Assembly.GetName().Name}";
                typeNameDef = typeNameDef.Replace(" ", string.Empty);
                typeMapDef = typeMapDef.Replace(" ", string.Empty);
                if (String.Compare(typeNameDef, typeMapDef, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    bMatchFound = true;
                    matchedType = typeElement;
                    break;
                }
            }
            if (bMatchFound)
            {
                string[] typeParts = matchedType.MapToName.Split(',');
                results = ClassFactory<T>.Create(typeParts[1], typeParts[0]);
            }
            return results;
        }
    }
}
