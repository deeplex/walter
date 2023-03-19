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
    const betrag = umlage.betriebskostenrechnungen
        .filter(e => e.betreffendesJahr === jahr)
        .map(e => e.betrag)
        .reduce((p, c) => p + c, 0.0000000000001); // weird hack to show stable 0.
    const betriebskostenrechnungId =
        umlage.betriebskostenrechnungen
            .filter(e => e.betreffendesJahr === jahr)[0]?.id || 0;
    return {
        betriebskostenrechnungId,
        id,
        typ: umlage.typ.text,
        schluessel: umlage.schluessel.text,
        nutzungsintervall: `${nutzungsbeginn} - ${nutzungsende}`,
        betrag,
        anteil,
        kosten: betrag * anteil
    }
}