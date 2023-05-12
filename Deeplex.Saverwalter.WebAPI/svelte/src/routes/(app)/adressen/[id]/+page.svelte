<script lang="ts">
  import {
    WalterAdresse,
    WalterGrid,
    WalterHeaderDetail,
    WalterKontakte,
    WalterLinks,
    WalterWohnungen,
    WalterZaehlerList
  } from '$WalterComponents';
  import type { WalterWohnungEntry } from '$WalterLib';
  import type { PageData } from './$types';

  export let data: PageData;
  const wohnungEntry: Partial<WalterWohnungEntry> = { adresse: { ...data.a } };
</script>

<WalterHeaderDetail
  S3URL={data.S3URL}
  a={data.a}
  apiURL={data.apiURL}
  title={data.a.anschrift}
  f={data.fetch}
/>

<WalterGrid>
  <WalterAdresse bind:value={data.a} />

  <WalterLinks>
    <WalterWohnungen
      kontakte={data.kontakte}
      a={wohnungEntry}
      title="Wohnungen"
      rows={data.a.wohnungen}
    />
    <WalterKontakte title="Personen" rows={data.a.kontakte} />
    <WalterZaehlerList
      umlagen={data.umlagen}
      zaehlertypen={data.zaehlertypen}
      wohnungen={data.wohnungen}
      title="Zähler"
      rows={data.a.zaehler}
    />
  </WalterLinks>
</WalterGrid>
