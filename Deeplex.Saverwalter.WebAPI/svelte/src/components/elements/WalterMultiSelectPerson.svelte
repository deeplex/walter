<script lang="ts">
    import type { WalterSelectionEntry } from "$walter/lib";
    import { walter_selection } from "$walter/services/requests";
    import { ContentSwitcher, Switch } from "carbon-components-svelte";
    import WalterQuickAddButton from "./WalterQuickAddButton.svelte";
    import WalterMultiSelect from "./WalterMultiSelect.svelte";
    import { WalterJuristischePerson, WalterNatuerlichePerson } from "..";

    export let value: WalterSelectionEntry[] | undefined;
    export let titleText: string;
    export let fetchImpl: typeof fetch;
    export let readonly: boolean = false;
    
    let entries = walter_selection.kontakte(fetchImpl);
    function onSubmit()
    {
        entries = walter_selection.kontakte(fetchImpl);
    }

    let addEntry = {};

    let personType = 0;
</script>

<WalterMultiSelect
    disabled={readonly}
    bind:value
    {titleText}
    {entries}/>

<WalterQuickAddButton
    bind:addEntry
    addUrl="/api/kontakte/{personType ? "jur" : "nat"}"
    {onSubmit}>
    <ContentSwitcher
        bind:selectedIndex={personType}>
        <Switch text="Natürliche Person" />
        <Switch text="Juristische Person" />
    </ContentSwitcher>
    {#if personType === 0}
        <WalterNatuerlichePerson entry={addEntry} {fetchImpl} />
    {:else}
        <WalterJuristischePerson entry={addEntry} {fetchImpl} />
    {/if}
</WalterQuickAddButton>
