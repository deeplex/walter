<script lang="ts">
    import {
        WalterAbrechnungEinheit,
        WalterAbrechnungGruppe,
        WalterAbrechnungResultat,
        WalterDatePicker
    } from '$walter/components';
    import { convertEuro } from '$walter/services/utils';
    import type { WalterBetriebskostenabrechnungKostengruppenEntry } from '$walter/types';
    import { Loading, Row, Tile } from 'carbon-components-svelte';

    export let abrechnung: WalterBetriebskostenabrechnungKostengruppenEntry;
</script>

{#await abrechnung}
    <div style="width:100%; display: flex; justify-content: center">
        <Loading withOverlay={false} />
    </div>
{:then}
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
        <WalterAbrechnungEinheit
            entry={gruppe}
            abrechnungstage={abrechnung.abrechnungszeitspanne}
        />
        <WalterAbrechnungGruppe
            rows={gruppe.kostenpunkte}
            year={abrechnung.jahr}
        />
        <Tile light>
            <h5 style="display: flex; justify-content: center">
                Zwischensumme: {convertEuro(gruppe.betragKalteNebenkosten)}
            </h5>
        </Tile>
    {/each}
{/await}
