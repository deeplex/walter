<script lang="ts">
  import { Accordion, Button } from 'carbon-components-svelte';
  import type { PageData } from './$types';

  import {
    WalterGrid,
    WalterWohnungen,
    WalterHeaderDetail,
    WalterBetriebskostenrechnung
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

  <Accordion>
    <WalterWohnungen
      kontakte={data.kontakte}
      title="Wohnungen"
      rows={data.a.wohnungen}
    />
  </Accordion>

  <Button href={`/umlagen/${data.a.umlage?.id}`}>Zur Umlage</Button>
</WalterGrid>