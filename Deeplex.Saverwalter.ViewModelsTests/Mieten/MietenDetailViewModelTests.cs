using Deeplex.Saverwalter.Model;
using FluentAssertions;
using System;
using Xunit;

namespace Deeplex.Saverwalter.ViewModels.Tests
{
    public class MietenDetailViewModelTests
    {
        [Fact]
        public void MietenDetailViewModelWhenCreatingWithoutMiete()
        {
            var stub = new MietenDetailViewModel();

            stub.Should().NotBeNull();

            stub.Entity.Should().NotBeNull();

            stub.Betrag.Should().Be(0);
            stub.BetreffenderMonat.Should().BeSameDateAs(DateTime.Now.AsUtcKind());
            stub.Zahlungsdatum.Should().BeSameDateAs(DateTime.Now.AsUtcKind());
            stub.Notiz.Should().BeNull();
            stub.VertragId.Should().Be(Guid.Empty);
        }

        [Theory]
        [InlineData(100, "June 06, 2015", "Jan 15, 2016", "Dies ist eine Notiz")]
        [InlineData(0, "December 31, 3000", "Jan 15, 1000", "")]
        public void MietenDetailViewModelWhenCreatingWithMiete(
            double betrag,
            DateTime betreffenderMonatString,
            DateTime zahlungsDatumString,
            string notiz)
        {
            var betreffenderMonat = Convert.ToDateTime(betreffenderMonatString);
            var zahlungsDatum = Convert.ToDateTime(zahlungsDatumString);

            var mock = new Miete()
            {
                Betrag = betrag,
                BetreffenderMonat = betreffenderMonat,
                Zahlungsdatum = zahlungsDatum,
                Notiz = notiz
            };
            var stub = new MietenDetailViewModel(mock);

            stub.Should().NotBeNull();

            stub.Entity.Should().NotBeNull();

            stub.Betrag.Should().Be(betrag);
            stub.BetreffenderMonat.Should().BeSameDateAs(betreffenderMonat);
            stub.Zahlungsdatum.Should().BeSameDateAs(zahlungsDatum);
            stub.Notiz.Should().Be(notiz);

            // VertragId is set by SQLite.
            stub.VertragId.Should().Be(Guid.Empty);
        }

        [Theory]
        [InlineData(100, "June 06, 2015", "Jan 15, 2016", "Dies ist eine Notiz")]
        [InlineData(0, "December 31, 3000", "Jan 15, 1000", "")]
        public void MietenDetailViewModelWhenSettingParameters(
         double betrag,
         DateTime betreffenderMonatString,
         DateTime zahlungsDatumString,
         string notiz)
        {
            var betreffenderMonat = Convert.ToDateTime(betreffenderMonatString);
            var zahlungsDatum = Convert.ToDateTime(zahlungsDatumString);

            var stub = new MietenDetailViewModel();

            stub.Betrag = betrag;
            stub.BetreffenderMonat = betreffenderMonat;
            stub.Zahlungsdatum = zahlungsDatum;
            stub.Notiz = notiz;

            stub.Should().NotBeNull();

            stub.Entity.Should().NotBeNull();

            stub.Betrag.Should().Be(betrag);
            stub.BetreffenderMonat.Should().BeSameDateAs(betreffenderMonat);
            stub.Zahlungsdatum.Should().BeSameDateAs(zahlungsDatum);
            stub.Notiz.Should().Be(notiz);

            // VertragId is set by SQLite.
            stub.VertragId.Should().Be(Guid.Empty);
        }
    }
}