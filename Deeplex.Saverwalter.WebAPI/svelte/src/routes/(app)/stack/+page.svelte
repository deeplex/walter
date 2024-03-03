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
    import { WalterAnhaenge, WalterHeader } from '$walter/components';
    import {
        Content,
        FileUploaderDropContainer,
        HeaderAction,
        HeaderUtilities
    } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { walter_file_post } from '$walter/services/files';
    import { openModal } from '$walter/store';
    import { WalterFileWrapper } from '$walter/lib';
    import { WalterFile } from '$walter/lib/WalterFile';

    export let data: PageData;

    let newFiles: File[] = [];

    function upload_finished(file: File) {
        if (data.files.some((e) => e.fileName == file.name)) {
            return;
        }
        data.files = [...data.files, WalterFile.fromFile(file, data.fileURL)];
    }

    function upload_file(file: File) {
        walter_file_post(file, data.fileURL, data.fetchImpl).then(() =>
            upload_finished(file)
        );
    }

    async function upload() {
        for (const file of newFiles) {
            {
                if (data.files.map((e) => e.fileName).includes(file.name)) {
                    const content = `Eine Datei mit dem Namen ${file.name} existiert bereits in dieser Ablage. Bist du sicher, dass diese Datei hochgeladen werden soll?`;
                    openModal({
                        modalHeading: `Datei existiert bereits`,
                        content,
                        primaryButtonText: 'Überschreiben',
                        submit: () => upload_file(file)
                    });
                } else {
                    upload_file(file);
                }
            }
        }
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
</script>

<WalterHeader title="Ablagestapel">
    <HeaderUtilities>
        <HeaderAction
            isOpen
            preventCloseOnClickOutside
            text="({data.files.length})"
        >
            <WalterAnhaenge bind:fileWrapper />
        </HeaderAction>
    </HeaderUtilities>
</WalterHeader>

<Content>
    <div class="FileUploaderDropContainerWrapper">
        <FileUploaderDropContainer
            multiple
            labelText="Hier klicken oder Dateien ablegen um sie hochzuladen."
            bind:files={newFiles}
            on:add={upload}
        />
    </div>
</Content>

<style>
    :global(
            .FileUploaderDropContainerWrapper .bx--file-browse-btn,
            .FileUploaderDropContainerWrapper .bx--file__drop-container
        ) {
        font-size: xx-large;
        flex: 0 0 97%;
        height: 85vh !important;
        max-width: none !important;
        align-items: center;
    }

    :global(.FileUploaderDropContainerWrapper .bx--file__drop-container) {
        padding-left: 20% !important;
    }
</style>
