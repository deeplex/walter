<script lang="ts">
  import type { PageData } from './$types';
  import {
    WalterHeaderDetail,
    WalterGrid,
    WalterZaehler,
    WalterZaehlerstaende,
    WalterUmlagen,
    WalterLinks
  } from '$WalterComponents';
  import { convertDateCanadian } from '$WalterServices/utils';
  import type { WalterZaehlerstandEntry } from '$WalterLib';

  export let data: PageData;

  const staende = data.a.staende;

  const lastZaehlerstand = staende.length
    ? staende[staende.length - 1]
    : undefined;
  const zaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {
    zaehler: { id: '' + data.a.id, text: data.a.kennnummer },
    datum: convertDateCanadian(new Date()),
    stand: lastZaehlerstand?.stand || 0,
    einheit: lastZaehlerstand?.einheit
  };
</script>

<WalterHeaderDetail
  S3URL={data.S3URL}
  files={data.anhaenge}
  a={data.a}
  apiURL={data.apiURL}
  title={data.a.kennnummer}
  f={data.fetch}
/>

<WalterGrid>
  <WalterZaehler
    umlagen={data.umlagen}
    zaehlertypen={data.zaehlertypen}
    wohnungen={data.wohnungen}
    a={data.a}
  />

  <WalterLinks>
    <WalterZaehlerstaende
      a={zaehlerstandEntry}
      title="Zählerstände"
      rows={data.a.staende}
    />
    <WalterUmlagen
      title="Umlagen"
      zaehler={data.zaehler}
      umlageschluessel={data.umlageschluessel}
      wohnungen={data.wohnungen}
      betriebskostentypen={data.betriebskostentypen}
      rows={data.a.umlagen}
    />
  </WalterLinks>
</WalterGrid>
