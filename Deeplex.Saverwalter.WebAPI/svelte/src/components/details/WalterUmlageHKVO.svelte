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
    import { Checkbox, Link, Row, Tile } from 'carbon-components-svelte';
    import WalterSlider from '../elements/WalterSlider.svelte';
    import WalterComboBox from '../elements/WalterComboBox.svelte';
    import WalterDatePicker from '../elements/WalterDatePicker.svelte';
    import {
        WalterUmlageEntry,
        WalterHKVOEntry,
        type WalterSelectionEntry
    } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { onMount } from 'svelte';

    export let entry: Partial<WalterUmlageEntry> = {};
    export let selectedWohnungen: WalterSelectionEntry[] | undefined;
    export let schluessel: WalterSelectionEntry | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    export let blockSave = false;

    let visible: boolean = false;
    let initialHKVO: Partial<WalterHKVOEntry> = {};
    let selectableUmlagen: Promise<WalterSelectionEntry[]> = new Promise(
        () => []
    );
    let selectableWaermezaehler: Promise<WalterSelectionEntry[]> =
        Promise.resolve([]);
    let seitWannHkvo: string | undefined;

    onMount(() => {
        visible = !!entry.hkvo;
        initialHKVO = { ...entry.hkvo };
        updateSelectableUmlagen();
        if (entry.id) {
            selectableWaermezaehler = walter_selection.waermezaehler(
                entry.id,
                fetchImpl
            );
        }
    });

    $: hasHkvoChanges =
        visible &&
        !!entry.hkvo &&
        !!initialHKVO.id &&
        (entry.hkvo.hkvO_P7 !== initialHKVO.hkvO_P7 ||
            entry.hkvo.hkvO_P8 !== initialHKVO.hkvO_P8 ||
            (entry.hkvo.hkvO_P9?.id ?? '') !==
                (initialHKVO.hkvO_P9?.id ?? '') ||
            entry.hkvo.strompauschale !== initialHKVO.strompauschale ||
            (entry.hkvo.stromrechnung?.id ?? '') !==
                (initialHKVO.stromrechnung?.id ?? '') ||
            (entry.hkvo.allgemeinWaerme?.id ?? '') !==
                (initialHKVO.allgemeinWaerme?.id ?? ''));

    $: blockSave = hasHkvoChanges && !seitWannHkvo;

    $: if (entry.hkvo && seitWannHkvo) {
        entry.hkvo.beginn = seitWannHkvo;
    }

    const hkvo_p9a2 = walter_selection.hkvo_p9a2(fetchImpl).then((res) => {
        // TODO: Implement Satz 1 and Satz 4
        (res[0] as WalterSelectionEntry & { disabled: boolean }).disabled =
            true;
        (res[2] as WalterSelectionEntry & { disabled: boolean }).disabled =
            true;

        return res;
    });

    let selectedWohnungenSet = new Set<number>(
        selectedWohnungen?.map((e) => +e.id)
    );
    function selectedWohnungenChanged() {
        const newSet = new Set<number>(selectedWohnungen?.map((e) => +e.id));
        if (
            selectedWohnungenSet.size === newSet.size &&
            [...selectedWohnungenSet].every((id) => newSet.has(id))
        ) {
            return false;
        } else {
            selectedWohnungenSet = newSet;
            return true;
        }
    }

    function updateSelectableUmlagen() {
        selectableUmlagen = WalterUmlageEntry.GetAll<WalterUmlageEntry>(
            fetchImpl
        ).then(async (umlagenReponse) => {
            const selectedWohnungenSet = new Set<number>(
                selectedWohnungen?.map((e) => +e.id)
            );

            const filteredUmlagen = umlagenReponse.filter((umlage) => {
                if (umlage.hkvo || umlage.id === entry.id) {
                    return false;
                }

                const wohnungenSet = new Set<number>(
                    umlage.selectedWohnungen.map((e) => +e.id)
                );
                return (
                    selectedWohnungenSet.size > 0 &&
                    [...selectedWohnungenSet].every((id) =>
                        wohnungenSet.has(id)
                    )
                );
            });

            const set3 = new Set<number>(filteredUmlagen.map((e) => +e.id));

            const allselectable = await walter_selection.umlagen(fetchImpl);
            const set4 = new Set<number>(allselectable.map((e) => +e.id));

            const ids = [...set3].filter((id) => set4.has(id));

            const selectable = allselectable.filter((e) => ids.includes(+e.id));

            return selectable;
        });
    }

    $: if (selectedWohnungenChanged()) {
        updateSelectableUmlagen();
    }

    function change(e: Event, p9a2: WalterSelectionEntry[]) {
        if ((e.target as HTMLInputElement).checked) {
            entry.hkvo = {
                id: initialHKVO.id || 0,
                hkvO_P7: initialHKVO.hkvO_P7 || 50,
                hkvO_P8: initialHKVO.hkvO_P8 || 50,
                hkvO_P9: initialHKVO.hkvO_P9 || p9a2[1],
                strompauschale: initialHKVO.strompauschale || 5,
                stromrechnung: initialHKVO.stromrechnung || undefined,
                allgemeinWaerme: initialHKVO.allgemeinWaerme || undefined
            };
        } else {
            entry.hkvo = undefined;
        }
        seitWannHkvo = undefined;
    }
</script>

{#if `${schluessel?.id}` === '3'}
    <Tile>
        <Row>
            {#await hkvo_p9a2 then p9a2}
                <div>
                    <Checkbox
                        bind:checked={visible}
                        on:change={(e) => change(e, p9a2)}
                    />
                </div>
                <p style="margin-top: 0.25em; margin-left: -0.5em">
                    Ist diese Umlage nach der
                    <Link href="https://www.gesetze-im-internet.de/heizkostenv/"
                        >Heizkostenverordnung</Link
                    >
                    umzulegen?
                </p>
            {/await}
        </Row>
        {#if entry.hkvo}
            <Row>
                <WalterComboBox
                    bind:value={entry.hkvo.stromrechnung}
                    {readonly}
                    required
                    bind:entries={selectableUmlagen}
                    titleText="Stromrechnung (Betriebsstrom)"
                />
                <WalterComboBox
                    bind:value={entry.hkvo.allgemeinWaerme}
                    {readonly}
                    bind:entries={selectableWaermezaehler}
                    titleText="AllgemeinWärme-Zähler (§9 Abs. 2)"
                />
            </Row>
            <Row>
                <WalterSlider
                    disabled={readonly}
                    step={5}
                    min={50}
                    max={70}
                    fullWidth
                    labelText="HKVO §7 (in %)"
                    bind:value={entry.hkvo.hkvO_P7}
                />
                <WalterSlider
                    disabled={readonly}
                    step={5}
                    min={50}
                    max={70}
                    fullWidth
                    labelText="HKVO §8 (in %)"
                    bind:value={entry.hkvo.hkvO_P8}
                />
                <WalterSlider
                    disabled={readonly}
                    step={1}
                    min={0}
                    max={10}
                    fullWidth
                    labelText="Strompauschale (in %)"
                    bind:value={entry.hkvo.strompauschale}
                />
                <WalterComboBox
                    required
                    {readonly}
                    entries={hkvo_p9a2}
                    bind:value={entry.hkvo.hkvO_P9}
                    titleText="HKVO §9"
                />
            </Row>
            {#if hasHkvoChanges}
                <Row>
                    <WalterDatePicker
                        required
                        bind:value={seitWannHkvo}
                        labelText="HKVO-Änderung gültig ab"
                    />
                </Row>
            {/if}
        {/if}
    </Tile>
{/if}
