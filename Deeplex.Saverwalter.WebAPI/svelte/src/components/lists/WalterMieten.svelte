<script lang="ts">
    import {
        WalterDataWrapper,
        WalterMiete,
        WalterNumberInput
    } from '$walter/components';
    import { WalterMieteEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'betreffenderMonat', value: 'Betreffender Monat' },
        { key: 'zahlungsdatum', value: 'Zahlungsdatum' },
        { key: 'betrag', value: 'Betrag' }
    ];

    export let rows: WalterMieteEntry[];
    const sortedRows = rows.sort((a, b) =>
        b.betreffenderMonat.localeCompare(a.betreffenderMonat)
    );
    export let fullHeight = false;
    export let title: string | undefined = undefined;

    const on_click_row = (e: CustomEvent) => navigation.miete(e.detail.id);

    export let entry: Partial<WalterMieteEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterMieteEntry.ApiURL}
    {on_click_row}
    addEntry={entry}
    {title}
    rows={sortedRows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterMiete {entry} />
        <WalterNumberInput
            label="Auch anwenden auf die nächsten Monate:"
            min={0}
            max={11}
            hideSteppers={false}
            bind:value={entry.repeat}
        />
    {/if}
</WalterDataWrapper>
