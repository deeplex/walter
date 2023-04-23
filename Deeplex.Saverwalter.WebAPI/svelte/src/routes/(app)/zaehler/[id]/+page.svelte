<script lang="ts">
  import { Accordion } from 'carbon-components-svelte';
  import type { PageData } from './$types';
  import {
    WalterHeaderDetail,
    WalterGrid,
    WalterZaehler,
    WalterZaehlerstaende,
    WalterZaehlerList
  } from '$WalterComponents';
  import { toLocaleIsoString } from '$WalterServices/utils';
  import type { WalterZaehlerEntry, WalterZaehlerstandEntry } from '$WalterLib';

  export let data: PageData;

  const staende = data.a.staende;

  const lastZaehlerstand = staende.length
    ? staende[staende.length - 1]
    : undefined;
  const zaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {
    zaehler: { id: '' + data.a.id, text: data.a.kennnummer },
    datum: toLocaleIsoString(new Date()),
    stand: lastZaehlerstand?.stand || 0,
    einheit: lastZaehlerstand?.einheit
  };

  const einzelzaehlerEntry: Partial<WalterZaehlerEntry> = {
    adresse: { ...data.a.adresse },
    typ: data.a.typ,
    allgemeinZaehler: { id: '' + data.a.id, text: data.a.kennnummer }
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
    zaehler={data.zaehler}
    zaehlertypen={data.zaehlertypen}
    wohnungen={data.wohnungen}
    a={data.a}
  />

  <Accordion>
    <WalterZaehlerstaende
      a={zaehlerstandEntry}
      title="Zählerstände"
      rows={data.a.staende}
    />
    <WalterZaehlerList
      zaehlertypen={data.zaehlertypen}
      zaehler={data.zaehler}
      wohnungen={data.wohnungen}
      a={einzelzaehlerEntry}
      title="Einzelzähler"
      rows={data.a.einzelzaehler}
    />
  </Accordion>
</WalterGrid>
