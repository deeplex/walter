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
        Button,
        InlineNotification,
        Modal,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListRow,
        TextInput
    } from 'carbon-components-svelte';
    import { Add, Subtract } from 'carbon-icons-svelte';
    import { WalterDatePicker, WalterNumberInput } from '$walter/components';
    import { WalterToastContent, WalterVertragEntry } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import { walter_get, walter_post } from '$walter/services/requests';
    import WalterRueckgaengig from './WalterRueckgaengig.svelte';
    import { addToast, changeTracker } from '$walter/store';
    import { get } from 'svelte/store';

    export let entry: Partial<WalterVertragEntry>;
    export let readonly = false;
    export let fetchImpl: typeof fetch = fetch;

    async function neuLaden() {
        if (!entry.id) return;
        const aktualisiert = WalterVertragEntry.fromJson(
            (await walter_get(
                `${WalterVertragEntry.ApiURL}/${entry.id}`,
                fetchImpl
            )) as WalterVertragEntry
        );
        entry.kaution = aktualisiert.kaution;
        entry.kautionsBuchungen = aktualisiert.kautionsBuchungen;
    }

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
    const datumText = (d: string) => new Date(d).toLocaleDateString('de-DE');

    $: gehalten = entry.kaution ?? 0;
    $: buchungen = entry.kautionsBuchungen ?? [];

    let open = false;
    let rueckzahlung = false;
    let datum: string | undefined;
    let betrag: number | undefined;
    let notiz = '';
    let busy = false;
    let fehler: string | null = null;

    // Die Eingabefelder melden Änderungen an den Seiten-Änderungszähler; der Dialog
    // stellt ihn beim Schließen wieder her, damit die Seite nicht als geändert gilt.
    let tracker = 0;
    let warOffen = false;
    $: if (open && !warOffen) {
        warOffen = true;
        tracker = get(changeTracker);
    } else if (!open && warOffen) {
        warOffen = false;
        changeTracker.set(tracker);
    }

    function oeffnen(alsRueckzahlung: boolean) {
        rueckzahlung = alsRueckzahlung;
        datum = convertDateCanadian(new Date());
        betrag = alsRueckzahlung ? gehalten : undefined;
        notiz = '';
        fehler = null;
        open = true;
    }

    $: gueltig =
        !!datum &&
        !!betrag &&
        betrag > 0 &&
        (!rueckzahlung || betrag <= gehalten + 0.005);

    async function buchen() {
        if (!entry.id || !gueltig) return;
        busy = true;
        fehler = null;
        const art = rueckzahlung ? 'Rückzahlung' : 'Eingang';
        const toast = new WalterToastContent(
            `Kaution ${art} gebucht`,
            `Fehler bei der Kaution`,
            () => `Kaution ${art} über ${eur(betrag ?? 0)} gebucht.`,
            (e: unknown) => `Konnte Kaution nicht buchen: ${e}`
        );
        try {
            const response = await walter_post(
                `${WalterVertragEntry.ApiURL}/${entry.id}/kaution`,
                { datum, betrag, rueckzahlung, notiz: notiz || null }
            );
            if (response.ok) {
                const aktualisiert = WalterVertragEntry.fromJson(
                    await response.json()
                );
                entry.kaution = aktualisiert.kaution;
                entry.kautionsBuchungen = aktualisiert.kautionsBuchungen;
                addToast(toast, true);
                open = false;
            } else {
                fehler = await response.text();
            }
        } finally {
            busy = false;
        }
    }
</script>

<p>
    Gehaltene Kaution: <strong>{eur(gehalten)}</strong>
</p>

{#if buchungen.length > 0}
    <StructuredList condensed style="margin: 0.5rem 0 1rem 0">
        <StructuredListBody>
            {#each buchungen as b}
                <StructuredListRow>
                    <StructuredListCell>{datumText(b.datum)}</StructuredListCell
                    >
                    <StructuredListCell>{b.beschreibung}</StructuredListCell>
                    <StructuredListCell style="text-align: right">
                        {eur(b.betrag)}
                    </StructuredListCell>
                    <StructuredListCell>
                        {#if b.transaktionId}
                            <a href={`/transaktionen/${b.transaktionId}`}
                                >Details →</a
                            >
                            {#if !readonly}
                                <WalterRueckgaengig
                                    {fetchImpl}
                                    transaktionId={b.transaktionId}
                                    on:erledigt={neuLaden}
                                />
                            {/if}
                        {/if}
                    </StructuredListCell>
                </StructuredListRow>
            {/each}
        </StructuredListBody>
    </StructuredList>
{/if}

{#if !readonly && entry.id}
    <div style="display: flex; gap: 0.5rem; margin-top: 0.5rem">
        <Button
            kind="tertiary"
            size="small"
            icon={Add}
            on:click={() => oeffnen(false)}
        >
            Kaution erhalten
        </Button>
        <Button
            kind="tertiary"
            size="small"
            icon={Subtract}
            disabled={gehalten <= 0}
            on:click={() => oeffnen(true)}
        >
            Kaution zurückzahlen
        </Button>
    </div>
{/if}

<Modal
    bind:open
    modalHeading={rueckzahlung ? 'Kaution zurückzahlen' : 'Kaution erhalten'}
    primaryButtonText="Buchen"
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={!gueltig || busy}
    on:click:button--secondary={() => (open = false)}
    on:submit={buchen}
>
    {#if open}
        {#if rueckzahlung}
            <p style="margin-bottom: 1rem">
                Gehaltene Kaution: <strong>{eur(gehalten)}</strong>. Die
                Rückzahlung darf diesen Betrag nicht übersteigen.
            </p>
        {/if}
        <WalterDatePicker required bind:value={datum} labelText="Datum" />
        <WalterNumberInput
            required
            hideSteppers
            label="Betrag (€)"
            max={rueckzahlung ? gehalten : undefined}
            bind:value={betrag}
        />
        <TextInput labelText="Notiz" bind:value={notiz} />
        {#if fehler}
            <InlineNotification
                kind="error"
                title="Buchung nicht möglich"
                subtitle={fehler}
                hideCloseButton
            />
        {/if}
    {/if}
</Modal>
