<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
        WalterDatePicker,
        WalterTextArea,
        WalterComboBoxKontakt,
        WalterComboBoxWohnung,
        WalterMultiSelectKontakt,
        WalterNumberInput
    } from '$walter/components';
    import type { WalterVertragEntry } from '$walter/lib';
    import { WalterToastContent, WalterVertragVersionEntry } from '$walter/lib';
    import {
        Row,
        TextInput,
        InlineNotification
    } from 'carbon-components-svelte';
    import { convertDateGerman } from '$walter/services/utils';
    import { walter_fetch, walter_post } from '$walter/services/requests';
    import { addToast } from '$walter/store';

    export let entry: Partial<WalterVertragEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    export let hasOverlap = false;
    export let blockSave = false;

    $: readonly = entry?.permissions?.update === false;
    $: isNew = !entry.id;

    // versionen[0] reference for new mode (same object — binds write straight through)
    $: newVersion = (entry.versionen?.[0] ??
        {}) as Partial<WalterVertragVersionEntry>;

    $: sortedVersionen = [...(entry.versionen ?? [])].sort((a, b) =>
        a.beginn.localeCompare(b.beginn)
    );
    $: firstVersion = sortedVersionen[0];
    $: latestVersion = sortedVersionen.at(-1);

    // Overlap check (new contracts only)
    let overlapConflict: {
        id: number;
        beginn: string;
        ende: string | null;
        mieter: string;
    } | null = null;

    async function checkOverlap(
        wohnungId: string | number | undefined,
        beginn: string | undefined,
        ende: string | undefined,
        excludeId: string | number | undefined
    ) {
        if (!fetchImpl || !wohnungId || !beginn) {
            overlapConflict = null;
            hasOverlap = false;
            return;
        }
        const params = new URLSearchParams({
            wohnungId: String(wohnungId),
            beginn
        });
        if (ende) params.set('ende', ende);
        if (excludeId) params.set('excludeId', String(excludeId));
        try {
            const r = await walter_fetch(
                fetchImpl,
                `/api/vertraege/check-overlap?${params}`
            );
            overlapConflict = await r.json();
        } catch {
            overlapConflict = null;
        }
        hasOverlap = !!overlapConflict;
    }

    $: if (isNew)
        checkOverlap(
            entry.wohnung?.id,
            newVersion.beginn,
            entry.ende,
            entry.id
        );

    // Unified display object for version fields.
    // New mode:  assigned = newVersion (same object reference → binds write into versionen[0]).
    // Edit mode: assigned = a copy of latestVersion → changes tracked for "since when" prompt.
    let displayVersion: Partial<WalterVertragVersionEntry> = {};
    let initialized = false;
    let seitWann: string | undefined;

    $: if (!initialized) {
        const source = isNew ? newVersion : latestVersion;
        if (source) {
            displayVersion = isNew
                ? newVersion
                : {
                      grundmiete: source.grundmiete,
                      nebenkostenvorauszahlung: source.nebenkostenvorauszahlung,
                      personenzahl: source.personenzahl
                  };
            initialized = true;
        }
    }

    $: hasVersionChanges =
        !isNew &&
        initialized &&
        (displayVersion.grundmiete !== latestVersion?.grundmiete ||
            displayVersion.nebenkostenvorauszahlung !==
                latestVersion?.nebenkostenvorauszahlung ||
            displayVersion.personenzahl !== latestVersion?.personenzahl);

    $: blockSave = hasVersionChanges && !seitWann;

    const eur = (v: number | undefined) =>
        v != null
            ? v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' })
            : '?';

    $: versionChanges = [
        displayVersion.grundmiete !== latestVersion?.grundmiete && {
            label: 'Grundmiete',
            from: eur(latestVersion?.grundmiete),
            to: eur(displayVersion.grundmiete)
        },
        displayVersion.nebenkostenvorauszahlung !==
            latestVersion?.nebenkostenvorauszahlung && {
            label: 'Nebenkostenvorauszahlung',
            from: eur(latestVersion?.nebenkostenvorauszahlung),
            to: eur(displayVersion.nebenkostenvorauszahlung)
        },
        displayVersion.personenzahl !== latestVersion?.personenzahl && {
            label: 'Personenzahl',
            from: String(latestVersion?.personenzahl ?? '?'),
            to: String(displayVersion.personenzahl ?? '?')
        }
    ].filter(Boolean) as { label: string; from: string; to: string }[];

    export let commitVersionIfPending: () => Promise<void> = async () => {};
    $: commitVersionIfPending = async () => {
        if (hasVersionChanges && seitWann) await createNewVersion();
    };

    async function createNewVersion() {
        if (!entry.id || !seitWann) return;

        const toast = new WalterToastContent(
            'Version gespeichert',
            'Fehler beim Speichern',
            () => 'Neue Vertragsversion erfolgreich erstellt.',
            (e: unknown) => `Konnte Version nicht speichern: ${e}`
        );

        const response = await walter_post(WalterVertragVersionEntry.ApiURL, {
            vertrag: { id: entry.id, text: '' },
            beginn: seitWann,
            grundmiete: displayVersion.grundmiete,
            nebenkostenvorauszahlung: displayVersion.nebenkostenvorauszahlung,
            personenzahl: displayVersion.personenzahl
        });
        const parsed = await response.json();
        addToast(toast, response.status === 200, parsed);

        if (response.status === 200) {
            entry.versionen = [
                ...(entry.versionen ?? []),
                parsed as WalterVertragVersionEntry
            ];
            initialized = false;
            seitWann = undefined;
        }
    }
</script>

<Row>
    {#if isNew}
        <WalterDatePicker
            required
            disabled={readonly}
            bind:value={newVersion.beginn}
            labelText="Beginn"
        />
    {:else}
        <TextInput
            required
            readonly
            labelText="Beginn"
            value={firstVersion?.beginn &&
                convertDateGerman(new Date(firstVersion.beginn))}
        />
    {/if}
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.ende}
        labelText="Ende"
        placeholder="Offen"
    />
</Row>
<Row>
    <WalterComboBoxWohnung
        {fetchImpl}
        required
        {readonly}
        bind:value={entry.wohnung}
    />
    <TextInput labelText="Vermieter" readonly value={entry.wohnung?.filter} />
    <WalterComboBoxKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.ansprechpartner}
        title="Ansprechpartner"
    />
</Row>
<Row>
    <WalterMultiSelectKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedMieter}
        titleText="Mieter"
    />
</Row>
<Row>
    <WalterNumberInput
        required
        {readonly}
        hideSteppers
        bind:value={displayVersion.grundmiete}
        label="Grundmiete"
    />
    <WalterNumberInput
        {readonly}
        hideSteppers
        bind:value={displayVersion.nebenkostenvorauszahlung}
        label="Nebenkostenvorauszahlung"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={displayVersion.personenzahl}
        label="Personenzahl"
    />
</Row>
{#if hasVersionChanges}
    <Row>
        <WalterDatePicker
            required
            bind:value={seitWann}
            labelText="Zeitpunkt der Änderung"
        />
    </Row>
{/if}
{#if overlapConflict}
    <InlineNotification kind="error" title="Konflikt:" hideCloseButton>
        <svelte:fragment slot="subtitle">
            Konflikt mit bestehendem Vertrag
            <a href="/vertraege/{overlapConflict.id}"
                >{overlapConflict.mieter || 'Unbekannt'}</a
            >
            vom {new Date(overlapConflict.beginn).toLocaleDateString('de-DE')}
            bis {overlapConflict.ende
                ? new Date(overlapConflict.ende).toLocaleDateString('de-DE')
                : 'offen'}.
        </svelte:fragment>
    </InlineNotification>
{/if}
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
