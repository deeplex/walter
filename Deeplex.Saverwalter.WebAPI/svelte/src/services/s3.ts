const baseURL = "http://192.168.178.61:9002/saverwalter";
type fetchType = (input: RequestInfo | URL, init?: RequestInit | undefined) => Promise<Response>

export const walter_s3_post = (file: File, path: string) => fetch(
    `${baseURL}/${path}/${file.name}`,
    {
        method: 'PUT',
        headers: {
            'Content-Type': `${file.type}`,
        },
        body: file
    }
);

export const walter_s3_get = (url: string) => fetch(
    `${baseURL}/${url}`, {
    method: 'GET',
    headers: {}
}).then(e => e.blob());

export function download_file_blob(blob: Blob, fileName: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

export function get_files_with_common_prefix(url: string, f: fetchType) {
    return f(`${baseURL}?prefix=${url}`, {
        method: 'GET',
        headers: {}
    })
        .then((e) => e.body?.getReader().read())
        .then((e) => {
            const json: any = e ? xml2json(new TextDecoder().decode(e.value)) : {};
            if (Array.isArray(json.Contents)) {
                const a = json.Contents
                    .map((e: any) => e.Key["#text"])
                    .map((e: string) => e.split("/").pop());
                console.log(a);
                return a;
            }
            else if (json.Contents?.Key) {
                return [json.Contents?.Key["#text"].split("/").pop()];
            }
            else {
                return [];
            }
        });
}

function xml2json(xmlString: string) {
    const parser = new DOMParser();
    const xml = parser.parseFromString(xmlString, 'text/xml');
    const person = xmlToJson(xml.documentElement);

    function xmlToJson(xml: Node): any {
        let obj: any = {};
        if (xml.nodeType === 1) {
            const element = xml as Element;
            if (element.attributes && element.attributes.length > 0) {
                obj['@attributes'] = {};
                for (let i = 0; i < element.attributes.length; i++) {
                    const attribute = element.attributes.item(i);
                    if (attribute) {
                        obj['@attributes'][attribute.nodeName] = attribute.nodeValue;
                    }
                }
            }
        } else if (xml.nodeType === 3) {
            obj = xml.nodeValue?.trim();
        }
        if (xml.hasChildNodes()) {
            for (let i = 0; i < xml.childNodes.length; i++) {
                const item = xml.childNodes.item(i);
                if (item) {
                    const nodeName = item.nodeName;
                    if (typeof obj[nodeName] === 'undefined') {
                        obj[nodeName] = xmlToJson(item);
                    } else {
                        if (typeof obj[nodeName].push === 'undefined') {
                            const old = obj[nodeName];
                            obj[nodeName] = [];
                            obj[nodeName].push(old);
                        }
                        obj[nodeName].push(xmlToJson(item));
                    }
                }
            }
        }
        return obj;
    }

    return person;
}
