<script lang="ts">
  import {
    WalterAbrechnungEinheit,
    WalterAbrechnungGruppe,
    WalterAbrechnungResultat,
    WalterDatePicker
  } from '$WalterComponents';
  import { convertEuro } from '$WalterServices/utils';
  import type { WalterBetriebskostenabrechnungKostengruppenEntry } from '$WalterTypes';
  import { Row, Tile } from 'carbon-components-svelte';

  export let abrechnung: WalterBetriebskostenabrechnungKostengruppenEntry;
</script>

{#await abrechnung then}
  <Row>
    <WalterDatePicker
      placeholder="Nutzungsbeginn"
      labelText="Nutzungsbeginn"
      disabled
      value={abrechnung.nutzungsbeginn.toLocaleString('de-DE')}
    />
    <WalterDatePicker
      placeholder="Nutzungsende"
      labelText="Nutzungsende"
      disabled
      value={abrechnung.nutzungsende.toLocaleString('de-DE')}
    />
  </Row>

  <WalterAbrechnungResultat entry={abrechnung} />
  {#each abrechnung.kostengruppen as gruppe}
    <hr />
    <WalterAbrechnungEinheit entry={gruppe} />
    <WalterAbrechnungGruppe rows={gruppe.kostenpunkte} year={abrechnung.jahr} />
    <Tile light>
      <h5 style="display: flex; justify-content: center">
        Zwischensumme: {convertEuro(gruppe.betragKalt)}
      </h5>
    </Tile>
  {/each}
{/await}
