import type { WalterBetriebskostenabrechnungKostenpunkt, WalterUmlageEntry } from "$WalterTypes";

const headers = {
    'Content-Type': 'application/octet-stream'
};

export function create_abrechnung(id: string, jahr: number, fileNameBase: string) {
    const apiURL = `/api/betriebskostenabrechnung/${id}/${jahr}`;
    const fileName = `Abrechnung ${jahr} - ${fileNameBase}.docx`;
    return fetch(apiURL, {
        method: 'GET',
        headers
    })
        .then((e) => e.blob())
        .then(e => new File([e], fileName));
}

export function getKostenpunkt(
    id: number,
    umlage: WalterUmlageEntry,
    nutzungsbeginn: string,
    nutzungsende: string,
    jahr: number,
    anteil: number): WalterBetriebskostenabrechnungKostenpunkt {
    const rechnungen = umlage.betriebskostenrechnungen
        .filter(e => e.betreffendesJahr === jahr)

    const betrag = rechnungen
        .map(e => e.betrag)
        .reduce((p, c) => p + c, 0.0000000000001); // weird hack to show stable 0.

    const betriebskostenrechnungId = rechnungen[rechnungen.length - 1]?.id || 0;
    return {
        betriebskostenrechnungId,
        umlageId: umlage.id,
        id,
        typ: umlage.typ,
        schluessel: umlage.schluessel,
        nutzungsintervall: `${nutzungsbeginn} - ${nutzungsende}`,
        betrag,
        anteil,
        kosten: betrag * anteil
    }
}