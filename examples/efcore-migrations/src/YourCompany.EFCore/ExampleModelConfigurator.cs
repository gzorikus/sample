using Microsoft.EntityFrameworkCore;
using YourCompany.Configuration.EFCore;
using YourCompany.EFCore.NeverPretendingToBeYourDomainModel;

namespace YourCompany.EFCore
{
    public sealed class ExampleModelConfigurator : YourCompanyDbContextConfigurator
    {
        public override void OnModelCreating(YourCompanyDbContextConfiguratorsLoadingContext context, ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EFCoreConventionalEntity>(entity =>
            {
                entity.OwnsMany(
                    one => one.TypicallyAlwaysIncludedCollectionIfYouAreGoingToBuildAggregatesOnTopOfIt,
                    many =>
                    {
                        many.Property<long>("Id").ValueGeneratedOnAdd();
                        many.HasKey("Id");

                        many.Property<int>("EntityId");
                        many.WithOwner().HasForeignKey("EntityId");
                    });

                entity.OwnsOne(
                    one => one.NonModularManualWayToMixIn,
                    another =>
                    {
                        another.Property<int>("EntityId");
                        another.HasKey("EntityId");
                        another.WithOwner().HasForeignKey("EntityId");
                    })
                    .Navigation(one => one.NonModularManualWayToMixIn)
                    .AutoInclude(false);

                entity.OwnsOne(
                    one => one.SomeReusableStuffLink,
                    another =>
                    {
                        another.HasKey(a => a.EntityId);
                        another.WithOwner().HasForeignKey(a => a.EntityId);

                        another.HasOne<EFCoreConventionalEntity.SomeReusableEntityHereForBrevity>()
                            .WithMany()
                            .HasForeignKey(o => o.ReusableEntityId);
                    })
                    .Navigation(one => one.SomeReusableStuffLink)
                    .AutoInclude(false);
            });

            modelBuilder.Entity<EFCoreConventionalEntity.SomeReusableEntityHereForBrevity>();
        }
    }
}