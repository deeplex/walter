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
    import {
        WalterBuchungssatz,
        WalterGrid,
        WalterHeaderDetail,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import { kontoVerknuepfungHref, type WalterAusgleich } from '$walter/lib';
    import { convertEuro, walter_goto } from '$walter/services/utils';
    import { formatToTableDate } from '$walter/components/elements/WalterDataTable';
    import { walter_delete, walter_post } from '$walter/services/requests';
    import {
        Button,
        InlineNotification,
        Modal,
        TextArea
    } from 'carbon-components-svelte';
    import { TrashCan, Undo } from 'carbon-icons-svelte';
    import type { PageData } from './$types';

    export let data: PageData;

    let stornoOpen = false;
    let stornoGrund = '';
    let loeschenOpen = false;
    let busy = false;
    let aktionsFehler: string | null = null;

    async function stornieren() {
        busy = true;
        aktionsFehler = null;
        try {
            const resp = await walter_post(
                `/api/buchungssaetze/${data.entry.id}/storno`,
                { grund: stornoGrund }
            );
            if (resp.ok) {
                const info = (await resp.json()) as { buchungssatzId: string };
                stornoOpen = false;
                await walter_goto(`/buchungssaetze/${info.buchungssatzId}`);
            } else {
                aktionsFehler = await resp.text();
                stornoOpen = false;
            }
        } finally {
            busy = false;
        }
    }

    async function loeschen() {
        busy = true;
        aktionsFehler = null;
        try {
            const resp = await walter_delete(
                `/api/buchungssaetze/${data.entry.id}`
            );
            if (resp.ok) {
                loeschenOpen = false;
                history.back();
            } else {
                aktionsFehler = await resp.text();
                loeschenOpen = false;
            }
        } finally {
            busy = false;
        }
    }

    let title = `Buchungssatz ${data.entry.nummer}`;
    $: {
        title = `Buchungssatz ${data.entry.nummer}`;
    }

    // OPOS-Ausgleiche aller Zeilen, pro Partner-Satz zusammengefasst —
    // so folgt man von der Forderung zur Zahlung und umgekehrt.
    $: ausgleiche = [
        ...data.entry.zeilen
            .flatMap((zeile) => zeile.ausgleiche)
            .reduce((map, ausgleich) => {
                const existing = map.get(ausgleich.buchungssatzId);
                if (existing) {
                    existing.betrag += ausgleich.betrag;
                } else {
                    map.set(ausgleich.buchungssatzId, { ...ausgleich });
                }
                return map;
            }, new Map<string, WalterAusgleich>())
            .values()
    ];

    // Eine Entität kann über mehrere Konten beteiligt sein — pro Entität
    // ein Link-Tile mit allen Funktionen.
    $: verknuepfungen = [
        ...data.entry.verknuepfungen
            .reduce((map, verknuepfung) => {
                const key = `${verknuepfung.typ}:${verknuepfung.id}`;
                const existing = map.get(key);
                if (existing) {
                    if (!existing.funktionen.includes(verknuepfung.funktion)) {
                        existing.funktionen.push(verknuepfung.funktion);
                    }
                } else {
                    map.set(key, {
                        ...verknuepfung,
                        funktionen: [verknuepfung.funktion]
                    });
                }
                return map;
            }, new Map<string, (typeof data.entry.verknuepfungen)[number] & { funktionen: string[] }>())
            .values()
    ];
</script>

<WalterHeaderDetail entry={data.entry} apiURL={data.apiURL} {title} />

<WalterGrid>
    <WalterBuchungssatz entry={data.entry} />

    {#if aktionsFehler}
        <InlineNotification
            kind="error"
            title="Korrektur nicht möglich:"
            subtitle={aktionsFehler}
            lowContrast
        />
    {/if}

    <div class="korrektur-aktionen">
        {#if data.entry.kannStornieren}
            <Button
                kind="danger-tertiary"
                icon={Undo}
                disabled={busy}
                on:click={() => (stornoOpen = true)}
            >
                Stornieren
            </Button>
        {/if}
        {#if data.entry.kannLoeschen}
            <Button
                kind="danger-tertiary"
                icon={TrashCan}
                disabled={busy}
                on:click={() => (loeschenOpen = true)}
            >
                Löschen
            </Button>
        {/if}
        {#if data.entry.sperrgrund}
            <InlineNotification
                kind="info"
                hideCloseButton
                title="Korrektur eingeschränkt:"
                subtitle={data.entry.sperrgrund}
                lowContrast
            />
        {/if}
    </div>

    <WalterLinks>
        {#if data.entry.stornoNach}
            <WalterLinkTile
                fileref=""
                name={`Storniert durch: Buchungssatz ${data.entry.stornoNach.buchungsjahr}-${data.entry.stornoNach.buchungsnummer}`}
                href={`/buchungssaetze/${data.entry.stornoNach.id}`}
            />
        {/if}
        {#if data.entry.stornoVon}
            <WalterLinkTile
                fileref=""
                name={`Storno von: Buchungssatz ${data.entry.stornoVon.buchungsjahr}-${data.entry.stornoVon.buchungsnummer}`}
                href={`/buchungssaetze/${data.entry.stornoVon.id}`}
            />
        {/if}
        {#if data.entry.transaktion}
            <WalterLinkTile
                fileref=""
                name={`Transaktion: ${data.entry.transaktion.text}`}
                href={`/transaktionen/${data.entry.transaktion.id}`}
            />
        {/if}
        {#each ausgleiche as ausgleich}
            <WalterLinkTile
                fileref=""
                name={`OPOS-Ausgleich: ${ausgleich.buchungsjahr}-${ausgleich.buchungsnummer} | ${formatToTableDate(ausgleich.buchungsdatum)} | ${ausgleich.beschreibung} — ${convertEuro(ausgleich.betrag)}`}
                href={`/buchungssaetze/${ausgleich.buchungssatzId}`}
            />
        {/each}
        {#each verknuepfungen as verknuepfung}
            <WalterLinkTile
                fileref=""
                name={`${verknuepfung.typ}: ${verknuepfung.text} (${verknuepfung.funktionen.join(', ')})`}
                href={kontoVerknuepfungHref(verknuepfung)}
            />
        {/each}
    </WalterLinks>
</WalterGrid>

<Modal
    bind:open={stornoOpen}
    danger
    modalHeading={`Buchungssatz ${data.entry.nummer} stornieren`}
    primaryButtonText="Stornieren"
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={busy || stornoGrund.trim().length === 0}
    on:click:button--secondary={() => (stornoOpen = false)}
    on:submit={stornieren}
>
    <p style="margin-bottom: 1rem;">
        Es wird eine Gegenbuchung mit umgekehrten Soll/Haben-Seiten erstellt.
        Bestehende OPOS-Ausgleiche dieses Satzes werden gelöst.
    </p>
    <TextArea
        labelText="Grund (Pflicht)"
        placeholder="Warum wird dieser Buchungssatz storniert?"
        bind:value={stornoGrund}
    />
</Modal>

<Modal
    bind:open={loeschenOpen}
    danger
    modalHeading={`Buchungssatz ${data.entry.nummer} löschen`}
    primaryButtonText="Endgültig löschen"
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={busy}
    on:click:button--secondary={() => (loeschenOpen = false)}
    on:submit={loeschen}
>
    <p>
        Der Buchungssatz „{data.entry.beschreibung}" wird endgültig entfernt.
        Das ist nur für frische Fehleingaben gedacht — im Zweifel lieber
        stornieren, damit die Korrektur nachvollziehbar bleibt.
    </p>
</Modal>

<style>
    .korrektur-aktionen {
        align-items: center;
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        padding: 0 1rem 1rem;
    }
</style>
