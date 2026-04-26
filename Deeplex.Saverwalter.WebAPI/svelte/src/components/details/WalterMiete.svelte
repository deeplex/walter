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
        WalterLinks,
        WalterMieten,
        WalterMonthPicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import {
        WalterMieteEntry,
        type WalterVertragEntry
    } from '$walter/lib';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import { fileURL } from '$walter/services/files';

    export let entry: Partial<WalterMieteEntry> = {};
    export let mieten: WalterMieteEntry[] = [];
    export let vertrag: WalterVertragEntry | undefined = undefined;
    export let vertraege: WalterVertragEntry[] = [];
    export let onDeleteMonthMiete:
        | ((deleted: WalterMieteEntry) => void)
        | undefined = undefined;
    export let onRequestCloseModal: (() => void) | undefined = undefined;
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
    $: {
        // Keep compatibility props in use while callers still pass them.
        onDeleteMonthMiete;
        onRequestCloseModal;
    }

    function dateToMonthKey(
        value: string | Date | undefined
    ): string | undefined {
        if (!value) {
            return undefined;
        }

        const parsed = new Date(value);
        if (Number.isNaN(parsed.getTime())) {
            return undefined;
        }

        const month = `${parsed.getMonth() + 1}`.padStart(2, '0');
        return `${parsed.getFullYear()}-${month}`;
    }

    function getGrundmieteForDate(
        vertragEntry: WalterVertragEntry,
        date: Date
    ) {
        const version = [...(vertragEntry.versionen || [])]
            .sort(
                (a, b) =>
                    new Date(a.beginn).getTime() - new Date(b.beginn).getTime()
            )
            .filter((versionEntry) => {
                const versionStart = new Date(versionEntry.beginn).getTime();
                return (
                    !Number.isNaN(versionStart) &&
                    versionStart <= date.getTime()
                );
            })
            .at(-1);

        return version?.grundmiete || 0;
    }

    $: monthKey = dateToMonthKey(entry.betreffenderMonat);
    $: entryVertragId = +(entry.vertrag?.id || 0);
    $: activeVertrag =
        vertrag ||
        vertraege.find((vertragEntry) => vertragEntry.id === entryVertragId);
    $: sameMonthMieten =
        monthKey && entryVertragId
            ? mieten
                  .filter((miete) => {
                      return (
                          +miete.vertrag?.id === entryVertragId &&
                          dateToMonthKey(miete.betreffenderMonat) === monthKey
                      );
                  })
                  .sort(
                      (a, b) =>
                          new Date(a.zahlungsdatum).getTime() -
                          new Date(b.zahlungsdatum).getTime()
                  )
            : [];
    $: otherMonthMieten = sameMonthMieten.filter(
        (miete) => miete.id !== +(entry.id || 0)
    );
    $: hasExistingMieteForSelectedMonth =
        (entry.id === undefined || entry.id === null) &&
        entryVertragId > 0 &&
        sameMonthMieten.length > 0;
    $: currentMonthDate = entry.betreffenderMonat
        ? new Date(entry.betreffenderMonat)
        : undefined;
    $: currentGrundmiete =
        activeVertrag &&
        currentMonthDate &&
        !Number.isNaN(currentMonthDate.getTime())
            ? getGrundmieteForDate(activeVertrag, currentMonthDate)
            : undefined;
    $: nebenkostenVorauszahlung =
        entry.betrag !== undefined && currentGrundmiete !== undefined
            ? +(entry.betrag - currentGrundmiete).toFixed(2)
            : undefined;
    $: sameMonthMietenKey = `${entryVertragId}-${monthKey || 'none'}-${otherMonthMieten
        .map((miete) => `${miete.id || 'new'}-${miete.betreffenderMonat || ''}`)
        .join(',')}`;
</script>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betrag}
        label="Betrag (Warmmiete)"
    />
    <WalterMonthPicker
        required
        disabled={readonly}
        bind:value={entry.betreffenderMonat}
        labelText="Betreffender Monat"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.zahlungsdatum}
        labelText="Zahlungsdatum"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
<Row>
    <WalterNumberInput
        readonly
        label="Aktuelle Kaltmiete (Vertrag)"
        value={currentGrundmiete}
    />
    <WalterNumberInput
        readonly
        label="Nebenkostenvorauszahlung (Warmmiete - Kaltmiete)"
        value={nebenkostenVorauszahlung}
    />
</Row>

<WalterLinks>
    <WalterLinkTile
        fileref={fileURL.vertrag(`${entry.vertrag?.id}`)}
        name={`Vertrag: ${mieten[0]?.vertrag?.text || 'ansehen'}`}
        href={`/vertraege/${entry.vertrag?.id}`}
    />
    {#if entry.vertrag?.id}
        {#key sameMonthMietenKey}
            <WalterMieten
                title={`Andere Mieten im ausgewählten Monat`}
                rows={otherMonthMieten}
            />
        {/key}
        <WalterMieten
            title="Mieten"
            rows={mieten.filter(
                (e) => +e.vertrag.id === +(entry.vertrag?.id || 0)
            )}
        />
    {/if}
</WalterLinks>
