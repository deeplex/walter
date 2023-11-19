<script lang="ts">
    import type { WalterSelectionEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { WalterComboBox, WalterWohnung } from '..';
    import WalterQuickAddButton from './WalterQuickAddButton.svelte';

    export let readonly: boolean = false;
    export let required: boolean = false;
    export let value: WalterSelectionEntry | undefined;
    export let fetchImpl: typeof fetch;

    let entries = walter_selection.wohnungen(fetchImpl);
    function onSubmit() {
        entries = walter_selection.wohnungen(fetchImpl);
    }

    const addEntry = {};
</script>

<div
    style="
    flex: 1 1 auto !important;
    display: flex !important;
    flex-wrap: wrap !important"
>
    <WalterComboBox
        {required}
        {readonly}
        bind:value
        titleText="Wohnung"
        {entries}
    />

    <WalterQuickAddButton
        title="Wohnungen"
        {addEntry}
        addUrl={'/api/wohnungen'}
        {onSubmit}
    >
        <WalterWohnung {fetchImpl} entry={addEntry} />
    </WalterQuickAddButton>
</div>
