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
        HeaderPanelDivider,
        InlineLoading
    } from 'carbon-components-svelte';
    import { WalterAnhaengeEntry } from '..';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import {
        groupFilesByYear,
        noYear,
        yearLabel,
        WalterToastContent,
        type WalterFileHandle
    } from '$walter/lib';
    import { fileURL, walter_file_change_year } from '$walter/services/files';
    import { openModal } from '$walter/store';
    import { upload_new_files } from './WalterAnhaenge';
    import {
        dragFileType,
        dragSourceType,
        dragging,
        hasYears,
        move_file_between_handles
    } from './WalterAnhaengeMove';

    export let permissions: WalterPermissions | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let allHandles: WalterFileHandle[];
    export let handle: WalterFileHandle;
    export let filter: string;

    // Der Ablagestapel gehört dem Nutzer; die Dateien eines Objekts darf nur ändern, wer es darf.
    $: canMove =
        handle.fileURL === fileURL.stack || !permissions || permissions.update;

    // Das Jahr bzw. die Ablage, über der gerade eine Datei schwebt.
    let dropYear: number | undefined;
    let dropHandle = false;

    // Solange eine Datei dieser Ablage gezogen wird, erscheint „Ohne Jahr“ als Ziel.
    let draggingHere = false;

    function dragStart(event: DragEvent, key: string) {
        draggingHere = true;
        event.dataTransfer?.setData(dragFileType, key);
        event.dataTransfer?.setData(dragSourceType, handle.fileURL);
        dragging.source = handle.fileURL;
    }

    function dragEnd() {
        draggingHere = false;
        dragging.source = undefined;
        dropYear = undefined;
        dropHandle = false;
    }

    function dragOver(event: DragEvent, year: number) {
        if (
            !canMove ||
            dragging.source !== handle.fileURL ||
            !event.dataTransfer?.types.includes(dragFileType)
        )
            return;
        event.preventDefault();
        dropYear = year;
    }

    // Dateien vom Rechner auf die Liste ziehen lädt sie hoch: auf ein Jahr in dieses Jahr, sonst
    // (Überschrift, Zwischenraum, Dateien ohne Jahr) ohne Jahr; die Ablage ohne Jahre kennt keins.
    let uploadZone: string | undefined;

    const isExternal = (event: DragEvent) =>
        !!event.dataTransfer?.types.includes('Files');

    function externalOver(event: DragEvent, zone: string) {
        if (!canMove || !isExternal(event)) return;
        event.preventDefault();
        event.stopPropagation();
        uploadZone = zone;
    }

    function externalLeave(event: DragEvent) {
        const zone = event.currentTarget as Element;
        if (!zone.contains(event.relatedTarget as Node | null))
            uploadZone = undefined;
    }

    async function externalDrop(event: DragEvent, year?: number) {
        if (!canMove || !isExternal(event)) return;
        event.preventDefault();
        event.stopPropagation();
        uploadZone = undefined;

        const files = [...(event.dataTransfer?.files ?? [])];
        if (files.length === 0) return;
        await upload_new_files(
            handle,
            files,
            fetchImpl,
            hasYears(handle) ? (year ?? noYear) : undefined
        );
        handle = handle;
        allHandles = allHandles;
    }

    // Eine Datei aus der anderen Ablage auf diese Überschrift ziehen verschiebt sie hierher.
    function dragOverHandle(event: DragEvent) {
        if (
            !canMove ||
            !dragging.source ||
            dragging.source === handle.fileURL ||
            !event.dataTransfer?.types.includes(dragFileType)
        )
            return;
        event.preventDefault();
        dropHandle = true;
    }

    async function dropOnHandle(event: DragEvent) {
        dropHandle = false;
        const key = event.dataTransfer?.getData(dragFileType);
        const sourceURL = event.dataTransfer?.getData(dragSourceType);
        if (!canMove || !key || !sourceURL || sourceURL === handle.fileURL)
            return;
        event.preventDefault();

        const source = allHandles.find((h) => h.fileURL === sourceURL);
        const file = (await source?.files)?.find((f) => f.key === key);
        if (!source || !file) return;

        const run = async () => {
            await move_file_between_handles(file, source, handle, fetchImpl);
            handle = handle;
            allHandles = allHandles;
        };
        if ((await handle.files).some((f) => f.fileName === file.fileName)) {
            openModal({
                modalHeading: 'Datei existiert bereits',
                content: `Eine Datei mit dem Namen ${file.fileName} existiert bereits in ${handle.name}. Soll sie überschrieben werden?`,
                primaryButtonText: 'Überschreiben',
                submit: run
            });
        } else {
            await run();
        }
    }

    // Eine Datei auf ein Jahr ziehen legt sie in dessen Ordner.
    async function drop(event: DragEvent, year: number) {
        dropYear = undefined;
        const key = event.dataTransfer?.getData(dragFileType);
        if (!canMove || !key || dragging.source !== handle.fileURL) return;
        event.preventDefault();

        const file = (await handle.files).find((f) => f.key === key);
        if (!file || file.year === year) return;

        const toast = new WalterToastContent(
            'Jahr geändert',
            'Jahr ändern fehlgeschlagen',
            () =>
                year > 0
                    ? `${file.fileName} liegt jetzt in ${year}.`
                    : `${file.fileName} liegt jetzt ohne Jahr.`,
            () =>
                `Konnte ${file.fileName} nicht nach ${yearLabel(year)} verschieben.`
        );
        const response = await walter_file_change_year(
            file,
            year,
            fetchImpl,
            toast
        );
        if (response.status === 200) {
            handle.files = handle.reload();
        }
    }
</script>

{#await handle.files}
    <HeaderPanelDivider>{handle.name} (-)</HeaderPanelDivider>
    <InlineLoading style="margin-left: 2em" description="Lade Dateien" />
{:then files}
    <div
        class="zone"
        class:upload={uploadZone === 'handle'}
        data-walter-dropzone
        role="presentation"
        on:dragover={(e) => externalOver(e, 'handle')}
        on:dragleave={externalLeave}
        on:drop={(e) => externalDrop(e)}
    >
        <div
            class="ablage"
            class:ziel={dropHandle}
            role="presentation"
            on:dragover={dragOverHandle}
            on:dragleave={() => (dropHandle = false)}
            on:drop={dropOnHandle}
        >
            <HeaderPanelDivider
                >{handle.name} ({files?.length || 0})</HeaderPanelDivider
            >
        </div>
        {#if draggingHere && hasYears(handle)}
            <li
                class="jahr"
                class:ziel={dropYear === 0}
                on:dragover={(e) => dragOver(e, 0)}
                on:dragleave={() => (dropYear = undefined)}
                on:drop={(e) => drop(e, 0)}
            >
                {yearLabel(0)}
            </li>
        {/if}
        {#each groupFilesByYear(files || []) as group (group.year)}
            {@const visible = group.files.filter((file) =>
                file.fileName.toLowerCase().includes(filter.toLowerCase())
            )}
            {#if visible.length > 0}
                <div
                    class="gruppe"
                    class:upload={uploadZone === `y${group.year}`}
                    data-walter-dropzone
                    role="presentation"
                    on:dragover={(e) => externalOver(e, `y${group.year}`)}
                    on:dragleave={externalLeave}
                    on:drop={(e) => externalDrop(e, group.year)}
                >
                    {#if group.year > 0}
                        <li
                            class="jahr"
                            class:ziel={dropYear === group.year}
                            on:dragover={(e) => dragOver(e, group.year)}
                            on:dragleave={() => (dropYear = undefined)}
                            on:drop={(e) => drop(e, group.year)}
                        >
                            {group.year} ({visible.length})
                        </li>
                    {/if}
                    {#each visible as file (file.key)}
                        <div
                            draggable={canMove}
                            on:dragstart={(e) => dragStart(e, file.key)}
                            on:dragend={dragEnd}
                            role="listitem"
                        >
                            <WalterAnhaengeEntry
                                bind:allHandles
                                {permissions}
                                {file}
                                {fetchImpl}
                                bind:handle
                            />
                        </div>
                    {/each}
                </div>
            {/if}
        {/each}
    </div>
{/await}

<style>
    .jahr {
        list-style: none;
        padding: 0.25rem 1rem;
        font-size: 0.75rem;
        color: #8d8d8d;
        border-bottom: 1px solid #393939;
    }
    .upload {
        outline: 1px dashed var(--cds-interactive, #0f62fe);
        outline-offset: -1px;
        background-color: rgba(15, 98, 254, 0.12);
    }
    .ablage.ziel {
        background-color: #393939;
    }
    .jahr.ziel {
        background-color: #393939;
        color: #f4f4f4;
    }
</style>
