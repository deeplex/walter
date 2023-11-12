<script lang="ts">
    import type { WalterSelectionEntry } from "$walter/lib";
    import { walter_selection } from "$walter/services/requests";
    import WalterQuickAddButton from "./WalterQuickAddButton.svelte";
    import WalterMultiSelect from "./WalterMultiSelect.svelte";
    import WalterUmlage from "../details/WalterUmlage.svelte";

    export let value: WalterSelectionEntry[] | undefined;
    export let titleText: string;
    export let fetchImpl: typeof fetch;
    export let readonly: boolean = false;
    
    let entries = walter_selection.umlagen(fetchImpl);
    function onSubmit()
    {
        entries = walter_selection.umlagen(fetchImpl);
    }

    let addEntry = {};
</script>

<WalterMultiSelect
    disabled={readonly}
    bind:value
    {titleText}
    {entries}/>

<WalterQuickAddButton
    bind:addEntry
    addUrl="/api/umlagen"
    {onSubmit}>
    <WalterUmlage entry={addEntry} {fetchImpl} />
</WalterQuickAddButton>
