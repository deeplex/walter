<!-- Copyright (C) 2023-2025  Kai Lawrence -->
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
        WalterAbrechnung,
        WalterGrid,
        WalterHeader,
        WalterLink
    } from '$walter/components';
    import { onMount } from 'svelte';
    import type { PageData } from './$types';
    import type {
        WalterBetriebskostenabrechnungEntry,
        WalterFile,
        WalterModalControl
    } from '$walter/types';
    import { page } from '$app/stores';
    import {
        Checkbox,
        ComboBox,
        InlineNotification,
        Loading,
        Modal,
        NumberInput,
        OverflowMenu,
        OverflowMenuItem,
        Row
    } from 'carbon-components-svelte';
    import { shouldFilterItem } from '$walter/components/elements/WalterComboBox';
    import { create_pdf_doc, create_word_doc, updatePreview } from './utils';
    import { Download } from 'carbon-icons-svelte';
    import type { WalterSelectionEntry } from '$walter/lib';
    import { download } from '$walter/components/preview/WalterPreview';
    import { walter_goto } from '$walter/services/utils';

    export let vertragId: number | null;
    export let selectedYear: number;

    let abrechnung: Promise<WalterBetriebskostenabrechnungEntry | undefined>;
    let title: string;
    const searchParams: URLSearchParams = new URL($page.url).searchParams;

    export let data: PageData;

    const likelyYear = new Date().getFullYear() - 1;

    let value: WalterSelectionEntry | undefined;

    let modalControl: WalterModalControl = {
        open: false,
        modalHeading: 'Abrechnung existiert bereits',
        content:
            'Es existiert bereits ein Ergebnis für die Abrechnung von diesem Vertrag in diesem Jahr. Möchtest du sie überschreiben?',
        danger: false,
        primaryButtonText: 'Bestätigen',
        submit: () => undefined
    };

    async function update() {
        walter_goto(`?${searchParams.toString()}`, { noScroll: true });
        abrechnung = updatePreview(vertragId, selectedYear, data.fetchImpl);

        const value = data.vertraege.find((vertrag) => vertrag.id == vertragId);
        title = value?.text || 'Wähle einen Vertrag aus';
    }

    onMount(async () => {
        vertragId = +(searchParams.get('vertrag') || 0) || null;
        selectedYear = +(searchParams.get('jahr') || 0) || likelyYear;
        value = data.vertraege.find((vertrag) => vertrag.id == vertragId);

        searchParams.set('jahr', `${selectedYear}`);

        update();
    });

    function select(e: CustomEvent) {
        vertragId = e.detail.selectedItem?.id || vertragId;
        if (vertragId) {
            searchParams.set('vertrag', `${vertragId}`);
        }
        update();
    }

    function change(e: CustomEvent<number | null>) {
        selectedYear = e.detail || likelyYear;
        searchParams.set('jahr', `${selectedYear}`);
        update();
    }

    async function create_file(
        submitFn: () => Promise<WalterFile | undefined>
    ): Promise<WalterFile | undefined> {
        let file;
        if (!vertragId || !selectedYear) return undefined;
        if ((await abrechnung)?.resultat) {
            modalControl.open = true;
            modalControl.submit = async () => {
                file = await submitFn();
                if (file) {
                    update();
                    download(file);
                }
            };
        } else {
            override_active = true;
            file = await submitFn();
        }

        return file;
    }

    let override_active = false;

    async function click_word(): Promise<void> {
        let submit = async () => {
            if (!vertragId || !selectedYear) return;
            let override = override_active;
            override_active = false;
            return await create_word_doc(
                vertragId,
                selectedYear,
                title,
                override,
                data.fetchImpl
            );
        };

        let file = await create_file(submit);

        if (file) {
            update();
            download(file);
        }
    }

    async function click_pdf(): Promise<void> {
        if (!vertragId || !selectedYear) return;

        let submit = async () => {
            if (!vertragId || !selectedYear) return;
            let override = override_active;
            override_active = false;
            return await create_pdf_doc(
                vertragId,
                selectedYear,
                title,
                override,
                data.fetchImpl
            );
        };

        let file = await create_file(submit);

        if (file) {
            update();
            download(file);
        }
    }
</script>

<WalterHeader {title} />

<Modal
    {...modalControl}
    bind:open={modalControl.open}
    secondaryButtonText="Abbrechen"
    on:click:button--secondary={() => (modalControl.open = false)}
    on:click:button--primary={() => (modalControl.open = false)}
    on:open
    on:close
    on:submit={modalControl.submit}
>
    <p>{modalControl.content}</p>
    <Checkbox
        labelText="Abrechnungsergebnis überschreiben"
        bind:checked={override_active}
        style="margin-top: 1em"
    />
</Modal>

<WalterGrid>
    <Row>
        <ComboBox
            placeholder="Wähle einen Vertrag aus..."
            selectedId={vertragId}
            on:select={select}
            items={data.vertraege}
            value={value?.text}
            titleText="Vertrag"
            {shouldFilterItem}
        />

        <div style="max-width: 15em">
            <NumberInput
                disabled={vertragId === null}
                on:change={change}
                bind:value={selectedYear}
                label="Jahr"
                hideSteppers={false}
            />
        </div>

        <OverflowMenu
            style="margin: auto; margin-bottom: 10px"
            flipped
            icon={Download}
        >
            <OverflowMenuItem on:click={click_word}
                >Word-Dokument erstellen</OverflowMenuItem
            >
            <OverflowMenuItem on:click={click_pdf}
                >PDF-Dokument erstellen</OverflowMenuItem
            >
        </OverflowMenu>
    </Row>

    {#await abrechnung}
        <Loading />
    {:then resolved}
        {#if resolved && resolved.zeitraum}
            {#if resolved.zeitraum.nutzungszeitraum > 0}
                <WalterAbrechnung
                    fetchImpl={data.fetchImpl}
                    abrechnung={resolved}
                    {title}
                />
            {:else}
                <InlineNotification
                    lowContrast
                    kind="error"
                    hideCloseButton
                    title="Abrechnungsjahr liegt außerhalb der Vertragslaufzeit."
                >
                    <WalterLink href={`vertraege/${vertragId}`}
                        >Klicke hier um zum Vertrag zu gelangen.</WalterLink
                    >
                </InlineNotification>
            {/if}
        {:else if vertragId}
            <InlineNotification lowContrast kind="error" hideCloseButton>
                Ups, da ist wohl irgendwas schiefgelaufen. Versuche die Seite
                neu zu laden...
            </InlineNotification>
        {/if}
    {/await}
</WalterGrid>
