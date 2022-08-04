using Deeplex.Saverwalter.Model;
using FluentAssertions;
using System;
using Xunit;

namespace Deeplex.Saverwalter.ViewModels.Tests
{
    public class MietenListViewModelEntryTests
    {
        [Fact(Skip = "TODO Mock impl, avm")]
        public void MietenDetailViewModelWhenCreatingWithoutMiete()
        {
            var mockMiete = new Miete();
            //var mockList = new MietenListViewModel();
            var stub = new MietenListViewModelEntry(mockMiete, null); // should be called with mockList

            stub.Should().NotBeNull();

            stub.Entity.Should().NotBeNull();

            stub.Betrag.Should().Be(0);
            stub.BetreffenderMonat.Value.Should().BeSameDateAs(DateTime.Now.AsUtcKind());
            stub.Zahlungsdatum.Value.Should().BeSameDateAs(DateTime.Now.AsUtcKind());
            stub.Notiz.Should().BeNull();
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

            var mockMiete = new Miete()
            {
                Betrag = betrag,
                BetreffenderMonat = betreffenderMonat,
                Zahlungsdatum = zahlungsDatum,
                Notiz = notiz
            };

            //var mockList = new MietenListViewModel();
            var stub = new MietenListViewModelEntry(mockMiete, null); // should be called with mockList

            stub.Should().NotBeNull();

            stub.Entity.Should().NotBeNull();

            stub.Betrag.Should().Be(betrag);
            stub.BetreffenderMonat.Value.Should().BeSameDateAs(betreffenderMonat);
            stub.Zahlungsdatum.Value.Should().BeSameDateAs(zahlungsDatum);
            stub.Notiz.Should().Be(notiz);
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

            var mockMiete = new Miete();
            //var mockList = new MietenListViewModel();
            var stub = new MietenListViewModelEntry(mockMiete, null);

            stub.Betrag.Value = betrag;
            stub.BetreffenderMonat.Value = betreffenderMonat;
            // Zahlungsdatum has no set
            // stub.Zahlungsdatum = zahlungsDatum;
            stub.Notiz.Value = notiz;

            stub.Should().NotBeNull();

            stub.Entity.Should().NotBeNull();

            stub.Betrag.Should().Be(betrag);
            stub.BetreffenderMonat.Value.Should().BeSameDateAs(betreffenderMonat);
            stub.Zahlungsdatum.Value.Should().BeSameDateAs(zahlungsDatum);
            stub.Notiz.Should().Be(notiz);
        }
    }
}