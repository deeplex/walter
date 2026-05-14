<!-- Copyright (C) 2023-2026  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import type {
        AbrechnungsresultatInfo,
        AbrechnungslaufGruppeResult
    } from './AbrechnungslaufTypes';
    import WalterAbrechnungslaufEinheit from './WalterAbrechnungslaufEinheit.svelte';
    import WalterAbrechnungslaufNebenkosten from './WalterAbrechnungslaufNebenkosten.svelte';
    import WalterAbrechnungslaufResultat from './WalterAbrechnungslaufResultat.svelte';
    import { convertEuro } from '$walter/services/utils';
    import {
        Button,
        ClickableTile,
        Column,
        OverflowMenu,
        OverflowMenuItem,
        Row,
        TextInput,
        Tile
    } from 'carbon-components-svelte';
    import Download from 'carbon-icons-svelte/lib/Download.svelte';
    import { walter_post } from '$walter/services/requests';
    import { download_file_blob } from '$walter/services/files';

    export let resultat: AbrechnungsresultatInfo;
    export let gruppe: AbrechnungslaufGruppeResult;
    export let jahr: number;

    let downloadLoading = false;
    let downloadError = '';

    async function herunterladen(format: 'pdf' | 'docx') {
        downloadLoading = true;
        downloadError = '';
        try {
            const response = await walter_post(
                `/api/abrechnungslauf/print/${format}`,
                {
                    wohnungIds: gruppe.wohnungIds,
                    jahr,
                    vertragId: resultat.vertragId
                }
            );
            if (!response.ok) {
                downloadError = await response.text();
                return;
            }
            const disposition =
                response.headers.get('content-disposition') ?? '';
            const match = disposition.match(
                /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/
            );
            const ext = format === 'docx' ? 'docx' : 'pdf';
            const fileName = match
                ? match[1].replace(/['"]/g, '')
                : `NK_${jahr}_Abrechnung.${ext}`;
            download_file_blob(await response.blob(), fileName);
        } finally {
            downloadLoading = false;
        }
    }

    const formatDate = (d: string | null) => {
        if (!d) return 'heute';
        const [y, m, day] = d.split('-');
        return `${day}.${m}.${y}`;
    };

    const result = resultat.vorauszahlung - resultat.rechnungsbetrag;

    const vertragsEinheiten = gruppe.abrechnungseinheiten.filter((e) =>
        e.nkZeilen.some((z) =>
            z.anteile.some((a) => a.vertragId === resultat.vertragId)
        )
    );

    const einheitenNk = vertragsEinheiten.map((einheit) => ({
        bezeichnung: einheit.wohnungNamen,
        betragKalt: einheit.nkZeilen
            .filter((z) =>
                z.anteile.some((a) => a.vertragId === resultat.vertragId)
            )
            .reduce((s, z) => {
                const a = z.anteile.find(
                    (a) => a.vertragId === resultat.vertragId
                );
                return s + z.betrag * (a?.anteilFaktor ?? 0);
            }, 0)
    }));
</script>

<!-- Buchungsstatus + Download -->
<div
    style="display: flex; align-items: stretch; gap: 0.5rem; margin-bottom: 0.5rem;"
>
    <div style="flex: 1;">
        {#if resultat.gebuchtesAbrechnungsResultat != null}
            <ClickableTile
                href="/abrechnungsresultate/{resultat.abrechnungsresultatId}"
            >
                <h4>Letzter Stand dieser Abrechnung:</h4>
                <p>
                    Saldo: {convertEuro(resultat.saldo)}
                    &nbsp;·&nbsp;
                    {resultat.abgesendet ? 'Abgesendet' : 'Nicht abgesendet'}
                </p>
            </ClickableTile>
        {:else}
            <Tile>Bisher keine Abrechnung erstellt</Tile>
        {/if}
    </div>

    <div style="display: flex; align-items: center;">
        <OverflowMenu
            icon={Download}
            iconDescription="Herunterladen"
            disabled={downloadLoading}
            flipped
        >
            <OverflowMenuItem
                text="PDF herunterladen"
                on:click={() => herunterladen('pdf')}
            />
            <OverflowMenuItem
                text="Word (.docx) herunterladen"
                on:click={() => herunterladen('docx')}
            />
        </OverflowMenu>
    </div>
</div>
{#if downloadError}
    <p
        style="color: var(--cds-support-error); font-size: 0.875rem; margin-bottom: 0.5rem;"
    >
        {downloadError}
    </p>
{/if}

<hr />

<Row>
    <Column>
        <Tile>
            <div style="display: flex; align-items: baseline; gap: 0.5ch;">
                <a href="/vertraege/{resultat.vertragId}">
                    <h4>
                        {resultat.wohnungBezeichnung
                            .split(' – ')
                            .slice(1)
                            .join(' – ')} – {resultat.mieterBezeichnung}
                    </h4>
                </a>
                <h4>- {jahr}</h4>
            </div>
        </Tile>
    </Column>
    <Column sm={1}>
        <Tile>
            <h4>Resultat: {convertEuro(Math.abs(result))}</h4>
            <p>{result > 0 ? 'bekommt der Mieter' : 'bekommt der Vermieter'}</p>
        </Tile>
    </Column>
</Row>

<Row>
    <TextInput
        labelText="Nutzungsbeginn"
        placeholder="Nutzungsbeginn"
        readonly
        value={formatDate(resultat.nutzungVon)}
    />
    <TextInput
        labelText="Nutzungsende"
        placeholder="Nutzungsende"
        readonly
        value={formatDate(resultat.nutzungBis)}
    />
</Row>

<Tile><h4>Kalte Nebenkosten</h4></Tile>

{#each vertragsEinheiten as einheit}
    <hr />
    <WalterAbrechnungslaufEinheit
        {einheit}
        vertragId={resultat.vertragId ?? 0}
        personenZeitanteile={resultat.personenZeitanteile}
        nutzungVon={resultat.nutzungVon}
        nutzungBis={resultat.nutzungBis}
        {jahr}
    />
{/each}

<Tile><h4>Gesamtergebnis der Abrechnung:</h4></Tile>
<WalterAbrechnungslaufResultat {resultat} {einheitenNk} />
