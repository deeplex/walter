import type { WalterUmlageEntry } from '$WalterLib';
import type { WalterBetriebskostenabrechnungEntry, WalterBetriebskostenabrechnungKostengruppenEntry, WalterBetriebskostenabrechnungKostenpunkt, WalterBetriebskostenabrechnungsRechnungsgruppe } from '$WalterTypes';
import { walter_fetch, walter_get } from './requests';
import { finish_s3_post } from './s3';

const headers = {
  'Content-Type': 'application/octet-stream'
};

export function create_abrechnung_word(
  vertrag_id: string,
  jahr: number,
  fileNameBase: string
) {
  const apiURL = `/api/betriebskostenabrechnung/${vertrag_id}/${jahr}/word_document`;
  const fileName = `Abrechnung ${jahr} - ${fileNameBase}.docx`;
  return create_abrechnung_file(apiURL, fileName);
}

export function create_abrechnung_pdf(
  vertrag_id: string,
  jahr: number,
  fileNameBase: string
) {
  const apiURL = `/api/betriebskostenabrechnung/${vertrag_id}/${jahr}/pdf_document`;
  const fileName = `Abrechnung ${jahr} - ${fileNameBase}.pdf`;
  return create_abrechnung_file(apiURL, fileName);
}

async function create_abrechnung_file(apiURL: string, fileName: string) {
  const fetchOptions = {
    method: 'GET',
    headers
  };
  const response = await walter_fetch(fetch, apiURL, fetchOptions);

  if (response.status === 200) {
    return response.blob().then((e) => new File([e], fileName));
  } else {
    return finish_s3_post(response);
  }
}

export async function loadAbrechnung(
  vertragId: string,
  year: string,
  fetchImpl: typeof fetch
) {
  const abrechnungURL = `/api/betriebskostenabrechnung/${vertragId}/${year}`;

  const abrechnung = await (walter_get(
    abrechnungURL,
    fetchImpl
  ) as Promise<WalterBetriebskostenabrechnungEntry>);

  return {
    ...abrechnung,
    kostengruppen: getKostengruppen(abrechnung)
  } as WalterBetriebskostenabrechnungKostengruppenEntry;
}

function getZeitanteil(rechnungsgruppe: WalterBetriebskostenabrechnungsRechnungsgruppe, umlage: WalterUmlageEntry) {
  switch (umlage.schluessel.id) {
    case "0":
      return rechnungsgruppe.wfZeitanteil;
    case "1":
      return rechnungsgruppe.neZeitanteil;
    case "2":
      return rechnungsgruppe.personenZeitanteil
        .reduce((pre, cur) => pre + cur.anteil, 0)
    case "3":
      console.warn(`Anzeige nach Verbrauch ist noch nicht implementiert. Dies ist nur vielleicht korrekt`);
      return Object.values(rechnungsgruppe.verbrauchAnteil)[0] as number
    case "4":
      return rechnungsgruppe.nfZeitanteil;
    default:
      console.error(`Umlageschlüssel ${umlage.schluessel.text} kann nicht zugeordnet werden.`);
      return 0;
  }
}

export function getKostengruppen(
  abrechnung: WalterBetriebskostenabrechnungEntry
) {
  return abrechnung.abrechnungseinheiten.map((abrechnungseinheit) => {
    const kostenpunkte = abrechnungseinheit.umlagen.map((umlageEntry, index) =>
      getKostenpunkt(
        index,
        umlageEntry,
        new Date(abrechnung.nutzungsbeginn).toLocaleDateString('de-De'),
        new Date(abrechnung.nutzungsende).toLocaleDateString('de-De'),
        abrechnung.jahr,
        getZeitanteil(abrechnungseinheit, umlageEntry)
      )
    );

    return {
      kostenpunkte,
      ...abrechnungseinheit
    };
  });
}

export function getKostenpunkt(
  id: number,
  umlage: WalterUmlageEntry,
  nutzungsbeginn: string,
  nutzungsende: string,
  jahr: number,
  anteil: number
): WalterBetriebskostenabrechnungKostenpunkt {
  const rechnungen = umlage.betriebskostenrechnungen.filter(
    (e) => e.betreffendesJahr === jahr
  );

  const betrag = rechnungen
    .map((e) => e.betrag)
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
  };
}
