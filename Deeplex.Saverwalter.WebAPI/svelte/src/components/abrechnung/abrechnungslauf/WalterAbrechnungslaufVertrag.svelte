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
        AbrechnungslaufGruppeResult,
        NkZeileInfo
    } from './AbrechnungslaufTypes';
    import { hkvoKosten } from './AbrechnungslaufTypes';
    import WalterAbrechnungslaufEinheit from './WalterAbrechnungslaufEinheit.svelte';
    import WalterAbrechnungslaufResultat from './WalterAbrechnungslaufResultat.svelte';
    import WalterMietOpos from '$walter/components/lists/WalterMietOpos.svelte';
    import type { MietOposMonat } from '$walter/components/lists/WalterMietOpos.svelte';
    import { convertEuro } from '$walter/services/utils';
    import {
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
    export let fetchImpl: typeof fetch;

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
            const fileName = match
                ? match[1].replace(/['"]/g, '')
                : `NK_${jahr}_Abrechnung.${format}`;
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

    $: wohnungName = resultat.wohnungBezeichnung
        .split(' – ')
        .slice(1)
        .join(' – ');
    $: result = resultat.vorauszahlung - resultat.rechnungsbetrag;
    $: resultLabel =
        result > 0 ? 'bekommt der Mieter' : 'bekommt der Vermieter';
    $: nutzungVonDisplay =
        resultat.nutzungVon <= `${jahr}-01-01`
            ? 'Jahresbeginn'
            : formatDate(resultat.nutzungVon);

    $: vertragsEinheiten = gruppe.abrechnungseinheiten.filter((e) =>
        e.nkZeilen.some((z) =>
            z.anteile.some((a) => a.vertragId === resultat.vertragId)
        )
    );

    $: nkNachEinheit = vertragsEinheiten.map((einheit) => {
        const vid = resultat.vertragId;
        const zeilen = einheit.nkZeilen.filter((z) =>
            z.anteile.some((a) => a.vertragId === vid)
        );
        const anteil = (z: NkZeileInfo) =>
            z.anteile.find((a) => a.vertragId === vid)!;
        const kalt = zeilen
            .filter((z) => z.para9_2 == null)
            .reduce((s, z) => s + z.betrag * anteil(z).anteilFaktor, 0);
        const warm = zeilen
            .filter((z) => z.para9_2 != null)
            .reduce((s, z) => s + hkvoKosten(z, anteil(z)), 0);
        return { bezeichnung: einheit.wohnungNamen, kalt, warm };
    });

    $: kalteNk = nkNachEinheit
        .filter((e) => e.kalt > 0)
        .map(({ bezeichnung, kalt }) => ({ bezeichnung, betrag: kalt }));

    $: warmeNk = nkNachEinheit
        .filter((e) => e.warm > 0)
        .map(({ bezeichnung, warm }) => ({ bezeichnung, betrag: warm }));

    $: mietOposRows = (() => {
        // Zuordnung über das BUCHUNGSJAHR des Satzes (nicht Buchungsdatum/Beschreibung):
        // migrierte Zahlungen sind oft auf ein späteres Datum gebucht, gehören aber
        // buchhalterisch ins Abrechnungsjahr. So werden bezahlte Mieten nicht mehr
        // fälschlich als offen gezeigt.
        const byJahr = new Map<number, { soll: number; haben: number }>();
        for (const m of resultat.mieten ?? []) {
            const cur = byJahr.get(m.buchungsjahr) ?? { soll: 0, haben: 0 };
            if (m.istSoll) cur.soll += m.betrag;
            else cur.haben += m.betrag;
            byJahr.set(m.buchungsjahr, cur);
        }
        return [...byJahr.entries()]
            .sort(([a], [b]) => a - b)
            .map(
                ([y, { soll, haben }]): MietOposMonat => ({
                    jahr: y,
                    monat: 0, // 0 = Jahres-Summe (kein einzelner Monat)
                    soll,
                    ausgeglichen: haben,
                    offen: soll - haben
                })
            );
    })();
</script>

<hr />
<!-- Buchungsstatus + Download -->
<div
    style="display: flex; align-items: stretch; gap: 0.5rem; margin-bottom: 0.5rem;"
>
    <div style="flex: 1;">
        {#if resultat.gebuchterSaldo != null}
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

<Row>
    <Column>
        <Tile light>
            <div style="display: flex; align-items: baseline; gap: 0.5ch;">
                <a href="/vertraege/{resultat.vertragId}">
                    <h4>{wohnungName} – {resultat.mieterBezeichnung}</h4>
                </a>
                <h4>- {jahr}</h4>
            </div>
        </Tile>
    </Column>
    <Column sm={1}>
        <Tile light>
            <h4>Resultat: {convertEuro(Math.abs(result))}</h4>
            <p>{resultLabel}</p>
        </Tile>
    </Column>
</Row>

<Row>
    <TextInput
        labelText="Nutzungsbeginn"
        placeholder="Nutzungsbeginn"
        readonly
        value={nutzungVonDisplay}
    />
    <TextInput
        labelText="Nutzungsende"
        placeholder="Nutzungsende"
        readonly
        value={formatDate(resultat.nutzungBis)}
    />
</Row>

<WalterMietOpos rows={mietOposRows} title="Mieten {jahr}" />

{#each vertragsEinheiten as einheit}
    <Tile style="margin-bottom: 1rem; padding: 0;">
        <WalterAbrechnungslaufEinheit
            {einheit}
            vertragId={resultat.vertragId ?? 0}
            personenZeitanteile={resultat.personenZeitanteile}
            nutzungVon={resultat.nutzungVon}
            nutzungBis={resultat.nutzungBis}
            {jahr}
            {fetchImpl}
        />
    </Tile>
{/each}

<Tile>
    <h4>Gesamtergebnis der Abrechnung:</h4>
    <WalterAbrechnungslaufResultat {resultat} {kalteNk} {warmeNk} />
</Tile>
