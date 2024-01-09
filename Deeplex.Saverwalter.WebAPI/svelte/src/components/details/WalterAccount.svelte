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
        Accordion,
        AccordionItem,
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
    <Accordion>
        <AccordionItem title="Verwaltete Wohnungen" open>
            <WalterMultiSelect
                hideLabel
                disabled={!admin}
                titleText="Verwaltete Wohnungen"
                bind:value={selectedWohnungen}
                entries={wohnungen}
            />
            <StructuredList flush condensed>
                <StructuredListHead>
                    <StructuredListRow head>
                        <StructuredListCell head>Wohnung</StructuredListCell>
                        <StructuredListCell head
                            >Berechtigung</StructuredListCell
                        >
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
        </AccordionItem>
    </Accordion>
</Row>
