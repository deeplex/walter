<script lang="ts">
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow
    } from 'carbon-components-svelte';
    import WalterTextInput from '../elements/WalterTextInput.svelte';
    import WalterMultiSelect from '../elements/WalterMultiSelect.svelte';
    import { walter_selection } from '$walter/services/requests';
    import type { WalterAccountEntry } from '$walter/lib';
    import WalterDropdown from '../elements/WalterDropdown.svelte';

    export let entry: Partial<WalterAccountEntry> = {};
    export let fetchImpl: typeof fetch;
    export let admin = false;

    let selectedWohnungen = entry.verwalter?.map((e) => e.wohnung) ?? [];
    const wohnungen = walter_selection.wohnungen(fetchImpl);
    const userrole = walter_selection.userrole(fetchImpl);
    const verwalterrollen = walter_selection.verwalterrollen(fetchImpl);

    $: {
        entry.verwalter = selectedWohnungen.map((wohnung) => {
            const id =
                entry.verwalter?.find((e) => e.wohnung.id === wohnung.id)?.rolle
                    .id || 0;
            return {
                wohnung,
                rolle: { id, text: 'hi' }
            };
        }) as any;
    }
</script>

<Row>
    <WalterTextInput
        required
        labelText="Nutzername"
        bind:value={entry.username}
    />
    <WalterDropdown
        required
        titleText="Rolle"
        entries={userrole}
        bind:value={entry.role}
    />
</Row>
<Row>
    <WalterTextInput required labelText="Anzeigename" bind:value={entry.name} />
</Row>

<Row>
    <WalterMultiSelect
        disabled={!admin}
        titleText="Verwaltete Wohnungen"
        bind:value={selectedWohnungen}
        entries={wohnungen}
    />
    <StructuredList condensed>
        <StructuredListHead>
            <StructuredListRow head>
                <StructuredListCell head>Wohnung</StructuredListCell>
                <StructuredListCell head>Berechtigung</StructuredListCell>
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            {#each entry.verwalter || [] as verwalter}
                <StructuredListRow>
                    <StructuredListCell
                        >{verwalter.wohnung.text}</StructuredListCell
                    >
                    <StructuredListCell>
                        <div style="width:14em">
                            <WalterDropdown
                                readonly={!admin}
                                required
                                hideLabel
                                titleText="Berechtigung"
                                entries={verwalterrollen}
                                bind:value={verwalter.rolle}
                            />
                        </div>
                    </StructuredListCell>
                </StructuredListRow>
            {/each}
        </StructuredListBody>
    </StructuredList>
</Row>
