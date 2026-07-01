using FantasyLoveSimAssetTool.Common;
using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Models
{
    public class EnemyProfile : ObservableObject
    {
        public int SchemaVersion { get; set; }

        public string EnemyId { get; set; }

        public string DisplayName { get; set; }

        public string EnemyType { get; set; }

        public string Memo { get; set; }

        public ObservableCollection<EnemyAsset> Assets { get; set; }

        public EnemyProfile()
        {
            SchemaVersion = 1;
            EnemyId = string.Empty;
            DisplayName = string.Empty;
            EnemyType = string.Empty;
            Memo = string.Empty;
            Assets = new ObservableCollection<EnemyAsset>();
        }
    }
}
