using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourCompany.EFCore.NeverPretendingToBeYourDomainModel
{
    [Table(TableName)]
    // an alternative is to define EFCore entitites in provider separated plugins to adapt to particular conventions along with migrations
    public class EFCoreConventionalEntity
    {
        public const string TableName = "Entity";

        public int Id { get; set; }

        public int Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }

        public List<HavingSomeOwnedCollection> TypicallyAlwaysIncludedCollectionIfYouAreGoingToBuildAggregatesOnTopOfIt { get; set; }
        public HavingSomeDataPutAwayForRareUseCases NonModularManualWayToMixIn { get; set; }
        public HavingToManuallyLinkEveryTime SomeReusableStuffLink { get; set; }

        // imagine what the ctor would look like eventually if we try "encapsulate" and make this a "domain model"

        [Table(TableName)]
        public class HavingSomeOwnedCollection
        {
            public const string TableName = "EntityOwnedCollection";

            public string Column1 { get; set; }
            public string Column2 { get; set; }
            public string Column3 { get; set; }
            public string Column4 { get; set; }
        }

        [Table(TableName)]
        public class HavingSomeDataPutAwayForRareUseCases
        {
            public const string TableName = "EntityMixin";

            public string Column1 { get; set; }
            public string Column2 { get; set; }
            public string Column3 { get; set; }
            public string Column4 { get; set; }
        }

        [Table(TableName)]
        public class HavingToManuallyLinkEveryTime
        {
            public const string TableName = "EntityLinkToReusableEntity";

            public int EntityId { get; set; }
            public int ReusableEntityId { get; set; }
        }

        [Table(TableName)]
        public class SomeReusableEntityHereForBrevity
        {
            public const string TableName = "ReusableEntity";

            public int Id { get; set; }

            public string WithCrossCuttingFeatureData { get; set; }
        }
    }
}