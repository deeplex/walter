<script lang="ts">
  import { Button } from 'carbon-components-svelte';
  import type { PageData } from './$types';

  import {
    WalterGrid,
    WalterWohnungen,
    WalterHeaderDetail,
    WalterBetriebskostenrechnung,
    WalterLinks
  } from '$WalterComponents';

  export let data: PageData;
</script>

<WalterHeaderDetail
  S3URL={data.S3URL}
  files={data.anhaenge}
  a={data.a}
  apiURL={data.apiURL}
  title={data.a.typ?.text +
    ' - ' +
    data.a.betreffendesJahr +
    ' - ' +
    data.a.umlage?.text}
  f={data.fetch}
/>

<WalterGrid>
  <WalterBetriebskostenrechnung
    betriebskostentypen={data.betriebskostentypen}
    umlagen={data.umlagen}
    a={data.a}
  />

  <WalterLinks>
    <WalterWohnungen
      kontakte={data.kontakte}
      title="Wohnungen"
      rows={data.a.wohnungen}
    />
  </WalterLinks>

  <Button href={`/umlagen/${data.a.umlage?.id}`}>Zur Umlage</Button>
</WalterGrid>
