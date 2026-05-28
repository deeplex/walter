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
    import {
        DataTable,
        Toolbar,
        ToolbarContent
    } from 'carbon-components-svelte';

    export let gruppe: AbrechnungslaufGruppeResult;
    export let jahr: number;

    const euro = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });

    const resultatStatus = (r: AbrechnungsresultatInfo): string => {
        if (r.gebuchtesAbrechnungsResultat === null) return 'Nicht gebucht';
        if (
            Math.abs(r.gebuchtesAbrechnungsResultat - r.rechnungsbetrag) > 0.005
        )
            return `Gebucht: ${euro(r.gebuchtesAbrechnungsResultat)}`;
        if (r.abgesendet) return 'Abgesendet';
        return 'Gebucht';
    };

    const saldoColor = (v: number) =>
        v !== 0 ? 'var(--cds-support-error)' : 'inherit';

    const splitWohnungBezeichnung = (value: string) => {
        const separator = ' – ';
        const splitAt = value.indexOf(separator);
        if (splitAt === -1) return { adresse: 'Ohne Adresse', wohnung: value };
        return {
            adresse: value.slice(0, splitAt),
            wohnung: value.slice(splitAt + separator.length)
        };
    };

    const formatDate = (d: string | null) => {
        if (!d) return 'heute';
        const [y, m, day] = d.split('-');
        return `${day}.${m}.${y}`;
    };

    const formatDateShort = (d: string) => {
        const [, m, day] = d.split('-');
        return `${day}.${m}`;
    };

    const getAbrechnungszeitraum = (
        nutzungVon: string,
        nutzungBis: string | null,
        abrechnungsJahr: number
    ) => {
        const jahrStart = `${abrechnungsJahr}-01-01`;
        const jahrEnde = `${abrechnungsJahr}-12-31`;
        const vonImJahr = nutzungVon < jahrStart ? jahrStart : nutzungVon;
        const bisRaw = nutzungBis ?? jahrEnde;
        const bisImJahr = bisRaw > jahrEnde ? jahrEnde : bisRaw;
        const von = bisImJahr < vonImJahr ? bisImJahr : vonImJahr;
        return {
            zeitraumImJahr: `${formatDateShort(von)} – ${formatDateShort(bisImJahr)}`,
            gesamtZeitraum: `${formatDate(nutzungVon)} – ${formatDate(nutzungBis)}`
        };
    };

    const sortResultate = (resultate: AbrechnungsresultatInfo[]) =>
        [...resultate].sort((a, b) => {
            const aW = splitWohnungBezeichnung(a.wohnungBezeichnung);
            const bW = splitWohnungBezeichnung(b.wohnungBezeichnung);
            const adressCompare = aW.adresse.localeCompare(bW.adresse, 'de-DE');
            if (adressCompare !== 0) return adressCompare;
            const wohnungCompare = aW.wohnung.localeCompare(
                bW.wohnung,
                'de-DE'
            );
            if (wohnungCompare !== 0) return wohnungCompare;
            const nutzungVonCompare = a.nutzungVon.localeCompare(b.nutzungVon);
            if (nutzungVonCompare !== 0) return nutzungVonCompare;
            const aBis = a.nutzungBis ?? '9999-12-31';
            const bBis = b.nutzungBis ?? '9999-12-31';
            return aBis.localeCompare(bBis);
        });

    const groupResultateByAdresse = (resultate: AbrechnungsresultatInfo[]) => {
        const result = new Map<string, AbrechnungsresultatInfo[]>();
        for (const item of sortResultate(resultate)) {
            const { adresse } = splitWohnungBezeichnung(
                item.wohnungBezeichnung
            );
            const list = result.get(adresse) ?? [];
            list.push(item);
            result.set(adresse, list);
        }
        return [...result.entries()].map(([adresse, items]) => ({
            adresse,
            resultate: items
        }));
    };

    const vertragResultate = sortResultate(
        gruppe.resultate.filter((r) => r.vertragId != null)
    );
    const leerstandResultate = gruppe.resultate.filter(
        (r) => r.vertragId == null
    );
</script>

<h4
    style="margin: 1.5rem 0 0.5rem; font-size: 1rem; font-weight: 600; color: var(--cds-text-primary);"
>
    {gruppe.gruppenBezeichnung}
</h4>

{#each groupResultateByAdresse(vertragResultate) as adressGruppe, adressIndex}
    <DataTable
        title={`Verträge - ${adressGruppe.adresse}`}
        description="NK-Abrechnung für {jahr}"
        headers={[
            { key: 'wohnungBezeichnung', value: 'Wohnung' },
            { key: 'mieterBezeichnung', value: 'Mieter' },
            { key: 'nutzung', value: 'Nutzungszeitraum' },
            { key: 'nkKontoSoll', value: 'Anteil' },
            { key: 'nkKontoHaben', value: 'Vorauszahlung' },
            { key: 'saldo', value: 'Ergebnis' },
            { key: 'status', value: 'Status' }
        ]}
        rows={adressGruppe.resultate.map((r, index) => ({
            id: `vertrag-${adressIndex}-${index}`,
            wohnungId: r.wohnungId,
            vertragId: r.vertragId,
            wohnungBezeichnung: splitWohnungBezeichnung(r.wohnungBezeichnung)
                .wohnung,
            mieterBezeichnung: r.mieterBezeichnung,
            nutzung: getAbrechnungszeitraum(r.nutzungVon, r.nutzungBis, jahr),
            nkKontoSoll: euro(r.rechnungsbetrag),
            nkKontoHaben: euro(r.vorauszahlung),
            saldo: { value: r.saldo, formatted: euro(r.saldo) },
            status: resultatStatus(r),
            statusVeraltet:
                r.gebuchtesAbrechnungsResultat !== null &&
                Math.abs(r.gebuchtesAbrechnungsResultat - r.rechnungsbetrag) >
                    0.005
        }))}
        size="short"
        style="margin-bottom: 1rem;"
    >
        <Toolbar><ToolbarContent /></Toolbar>
        <svelte:fragment slot="cell" let:cell let:row>
            {#if cell.key === 'wohnungBezeichnung'}
                <a href="/wohnungen/{row.wohnungId}">{cell.value}</a>
            {:else if cell.key === 'mieterBezeichnung'}
                <a href="/vertraege/{row.vertragId}">{cell.value}</a>
            {:else if cell.key === 'saldo'}
                <span
                    style="font-weight: 600; color: {saldoColor(
                        cell.value.value
                    )};"
                >
                    {cell.value.formatted}
                </span>
            {:else if cell.key === 'status'}
                <span
                    style={row.statusVeraltet
                        ? 'color: var(--cds-support-error); font-weight: 600;'
                        : ''}
                >
                    {cell.value}
                </span>
            {:else if cell.key === 'nutzung'}
                <span title={cell.value.gesamtZeitraum}
                    >{cell.value.zeitraumImJahr}</span
                >
            {:else}
                {cell.value}
            {/if}
        </svelte:fragment>
    </DataTable>
{/each}

{#each groupResultateByAdresse(leerstandResultate) as adressGruppe, adressIndex}
    <DataTable
        title={`Wohnungen - ${adressGruppe.adresse}`}
        description="Leerstand / Eigenanteile für {jahr}"
        headers={[
            { key: 'wohnungBezeichnung', value: 'Wohnung' },
            { key: 'leerstand', value: 'Leerstand' },
            { key: 'kosten', value: 'Kosten' },
            { key: 'status', value: 'Status' }
        ]}
        rows={adressGruppe.resultate.map((r, index) => ({
            id: `wohnung-${adressIndex}-${index}`,
            wohnungId: r.wohnungId,
            wohnungBezeichnung: splitWohnungBezeichnung(r.wohnungBezeichnung)
                .wohnung,
            leerstand: getAbrechnungszeitraum(r.nutzungVon, r.nutzungBis, jahr),
            kosten: euro(r.rechnungsbetrag),
            status: resultatStatus(r),
            statusVeraltet:
                r.gebuchtesAbrechnungsResultat !== null &&
                Math.abs(r.gebuchtesAbrechnungsResultat - r.rechnungsbetrag) >
                    0.005
        }))}
        size="short"
        style="margin-bottom: 0.5rem;"
    >
        <Toolbar><ToolbarContent /></Toolbar>
        <svelte:fragment slot="cell" let:cell let:row>
            {#if cell.key === 'wohnungBezeichnung'}
                <a href="/wohnungen/{row.wohnungId}">{cell.value}</a>
            {:else if cell.key === 'status'}
                <span
                    style={row.statusVeraltet
                        ? 'color: var(--cds-support-error); font-weight: 600;'
                        : ''}
                >
                    {cell.value}
                </span>
            {:else if cell.key === 'leerstand'}
                <span title={cell.value.gesamtZeitraum}
                    >{cell.value.zeitraumImJahr}</span
                >
            {:else}
                {cell.value}
            {/if}
        </svelte:fragment>
    </DataTable>
{/each}
