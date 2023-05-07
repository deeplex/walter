import type { WalterUmlageEntry } from '$WalterLib';
import type { WalterBetriebskostenabrechnungEntry, WalterBetriebskostenabrechnungKostengruppenEntry, WalterBetriebskostenabrechnungKostenpunkt } from '$WalterTypes';
import { walter_get } from './requests';
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
  const response = await fetch(apiURL, fetchOptions);

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

export function getKostengruppen(
  abrechnung: WalterBetriebskostenabrechnungEntry
) {
  return abrechnung.gruppen.map((e) => {
    const kostenpunkte = e.umlagen.map((u, i) =>
      getKostenpunkt(
        i,
        u,
        new Date(abrechnung.nutzungsbeginn).toLocaleDateString('de-De'),
        new Date(abrechnung.nutzungsende).toLocaleDateString('de-De'),
        abrechnung.jahr,
        e.wfZeitanteil
      )
    );

    return {
      kostenpunkte,
      ...e
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
